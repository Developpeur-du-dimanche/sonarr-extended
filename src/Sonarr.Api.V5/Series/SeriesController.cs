using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Aliases;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V5.Series;

[V5ApiController]
public class SeriesController : RestControllerWithSignalR<SeriesResource, NzbDrone.Core.Tv.Series>,
                            IHandle<EpisodeImportedEvent>,
                            IHandle<EpisodeFileDeletedEvent>,
                            IHandle<SeriesUpdatedEvent>,
                            IHandle<SeriesEditedEvent>,
                            IHandle<SeriesDeletedEvent>,
                            IHandle<SeriesRenamedEvent>,
                            IHandle<SeriesBulkEditedEvent>,
                            IHandle<MediaCoversUpdatedEvent>
{
    private readonly ISeriesService _seriesService;
    private readonly IAddSeriesService _addSeriesService;
    private readonly ISeriesStatisticsService _seriesStatisticsService;
    private readonly ISceneMappingService _sceneMappingService;
    private readonly ISeriesAliasService _seriesAliasService;
    private readonly IMapCoversToLocal _coverMapper;
    private readonly IManageCommandQueue _commandQueueManager;
    private readonly IRootFolderService _rootFolderService;

    private readonly LockByIdPool _seriesLockPool = new();

    public SeriesController(IBroadcastSignalRMessage signalRBroadcaster,
                        ISeriesService seriesService,
                        IAddSeriesService addSeriesService,
                        ISeriesStatisticsService seriesStatisticsService,
                        ISceneMappingService sceneMappingService,
                        ISeriesAliasService seriesAliasService,
                        IMapCoversToLocal coverMapper,
                        IManageCommandQueue commandQueueManager,
                        IRootFolderService rootFolderService,
                        RootFolderValidator rootFolderValidator,
                        MappedNetworkDriveValidator mappedNetworkDriveValidator,
                        SeriesPathValidator seriesPathValidator,
                        SeriesExistsValidator seriesExistsValidator,
                        SeriesAncestorValidator seriesAncestorValidator,
                        SystemFolderValidator systemFolderValidator,
                        QualityProfileExistsValidator qualityProfileExistsValidator,
                        RootFolderExistsValidator rootFolderExistsValidator,
                        SeriesFolderAsRootFolderValidator seriesFolderAsRootFolderValidator)
        : base(signalRBroadcaster)
    {
        _seriesService = seriesService;
        _addSeriesService = addSeriesService;
        _seriesStatisticsService = seriesStatisticsService;
        _sceneMappingService = sceneMappingService;
        _seriesAliasService = seriesAliasService;

        _coverMapper = coverMapper;
        _commandQueueManager = commandQueueManager;
        _rootFolderService = rootFolderService;

        SharedValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
            .IsValidPath()
            .SetValidator(rootFolderValidator)
            .SetValidator(mappedNetworkDriveValidator)
            .SetValidator(seriesPathValidator)
            .SetValidator(seriesAncestorValidator)
            .SetValidator(systemFolderValidator)
            .When(s => s.Path.IsNotNullOrWhiteSpace());

        PostValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .IsValidPath()
            .When(s => s.RootFolderPath.IsNullOrWhiteSpace());
        PostValidator.RuleFor(s => s.RootFolderPath).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .IsValidPath()
            .SetValidator(rootFolderExistsValidator)
            .SetValidator(seriesFolderAsRootFolderValidator)
            .When(s => s.Path.IsNullOrWhiteSpace());

        PutValidator.RuleFor(s => s.Path).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .IsValidPath();

        SharedValidator.RuleFor(s => s.QualityProfileId).Cascade(CascadeMode.Stop)
            .ValidId()
            .SetValidator(qualityProfileExistsValidator);

        PutValidator.RuleFor(s => s.Aliases).Custom((aliases, context) =>
        {
            if (aliases == null)
            {
                return;
            }

            var resource = (SeriesResource)context.InstanceToValidate;

            foreach (var alias in aliases.Where(a => IsAliasUsedByOtherSeries(resource, a)))
            {
                context.AddFailure("Aliases", $"'{alias}' is already used by another series");
            }

            foreach (var alias in aliases.Where(a => IsAliasAlreadyKnownForSeries(resource, a)))
            {
                context.AddFailure("Aliases", $"'{alias}' is already a known title for this series");
            }
        });

        PostValidator.RuleFor(s => s.Title).NotEmpty();
        PostValidator.RuleFor(s => s.TvdbId).GreaterThan(0).SetValidator(seriesExistsValidator);
    }

    [HttpGet]
    [Produces("application/json")]
    public Ok<List<SeriesResource>> AllSeries(int? tvdbId, [FromQuery] SeriesSubresource[]? includeSubresources = null)
    {
        var seriesStats = _seriesStatisticsService.SeriesStatistics();
        var seriesResources = new List<SeriesResource>();
        var includeSeasonImages = includeSubresources.Contains(SeriesSubresource.SeasonImages);

        if (tvdbId.HasValue)
        {
            seriesResources.AddIfNotNull(_seriesService.FindByTvdbId(tvdbId.Value)?.ToResource(includeSeasonImages));
        }
        else
        {
            seriesResources.AddRange(_seriesService.GetAllSeries().Select(s => s.ToResource(includeSeasonImages)));
        }

        MapCoversToLocal(seriesResources.ToArray());
        LinkSeriesStatistics(seriesResources, seriesStats.ToDictionary(x => x.SeriesId));
        PopulateAlternateTitles(seriesResources);
        PopulateAliases(seriesResources);
        seriesResources.ForEach(LinkRootFolderPath);

        return TypedResults.Ok(seriesResources);
    }

    [NonAction]
    public override Results<Ok<SeriesResource>, NotFound> GetResourceByIdWithErrorHandler(int id)
    {
        return base.GetResourceByIdWithErrorHandler(id);
    }

    [RestGetById]
    [Produces("application/json")]
    public Results<Ok<SeriesResource>, NotFound> GetResourceByIdWithErrorHandler(int id, [FromQuery] SeriesSubresource[]? includeSubresources = null)
    {
        var includeSeasonImages = includeSubresources.Contains(SeriesSubresource.SeasonImages);

        try
        {
            var series = GetSeriesResourceById(id, includeSeasonImages);

            return series == null ? TypedResults.NotFound() : TypedResults.Ok(series);
        }
        catch (ModelNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    protected override SeriesResource? GetResourceById(int id)
    {
        var includeSubresources = Request?.Query["includeSubresources"].Select(v =>
        {
            if (Enum.TryParse<SeriesSubresource>(v, true, out var enumValue))
            {
                return enumValue;
            }

            throw new BadRequestException($"The value '{v}' is not valid.");
        }) ?? [];

        var includeSeasonImages = includeSubresources.Contains(SeriesSubresource.SeasonImages);

        return GetSeriesResourceById(id, includeSeasonImages);
    }

    private SeriesResource? GetSeriesResourceById(int id, bool includeSeasonImages)
    {
        var series = _seriesService.GetSeries(id);

        return GetSeriesResource(series, includeSeasonImages);
    }

    [RestPostById]
    [Consumes("application/json")]
    [Produces("application/json")]
    public Results<Created<SeriesResource>, NotFound> AddSeries([FromBody] SeriesResource seriesResource)
    {
        var series = _addSeriesService.AddSeries(seriesResource.ToModel());

        return TypedCreated(series.Id);
    }

    [RestPutById]
    [Consumes("application/json")]
    [Produces("application/json")]
    public Results<Accepted<SeriesResource>, NotFound> UpdateSeries([FromBody] SeriesResource seriesResource, [FromQuery] bool moveFiles = false)
    {
        var series = _seriesService.GetSeries(seriesResource.Id);

        if (moveFiles)
        {
            var sourcePath = series.Path;
            var destinationPath = seriesResource.Path;

            _commandQueueManager.Push(new MoveSeriesCommand
            {
                SeriesId = series.Id,
                SourcePath = sourcePath,
                DestinationPath = destinationPath
            },
                trigger: CommandTrigger.Manual);
        }

        if (seriesResource.Aliases != null)
        {
            _seriesAliasService.SetAliases(series.Id, seriesResource.Aliases);
        }

        var model = seriesResource.ToModel(series);

        _seriesService.UpdateSeries(model);

        PopulateAlternateTitles(seriesResource);
        PopulateAliases(seriesResource);
        BroadcastResourceChange(ModelAction.Updated, seriesResource);

        return TypedAccepted(seriesResource.Id);
    }

    [HttpPut("{id:int}/season")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public Results<Ok<SeasonResource>, NotFound> UpdateSeasonMonitored([FromRoute] int id, [FromBody] SeasonResource seasonResource)
    {
        lock (_seriesLockPool.GetLock(id))
        {
            var series = _seriesService.GetSeries(id);
            var season = series.Seasons.FirstOrDefault(s => s.SeasonNumber == seasonResource.SeasonNumber);

            if (season == null)
            {
                return TypedResults.NotFound();
            }

            season.Monitored = seasonResource.Monitored;

            _seriesService.UpdateSeries(series);

            BroadcastResourceChange(ModelAction.Updated, GetSeriesResource(series, false)!);

            return TypedResults.Ok(season.ToResource());
        }
    }

    [RestDeleteById]
    public NoContent DeleteSeries(int id, bool deleteFiles = false, bool addImportListExclusion = false)
    {
        _seriesService.DeleteSeries(new List<int> { id }, deleteFiles, addImportListExclusion);

        return TypedResults.NoContent();
    }

    private SeriesResource? GetSeriesResource(NzbDrone.Core.Tv.Series? series, bool includeSeasonImages)
    {
        if (series == null)
        {
            return null;
        }

        var resource = series.ToResource(includeSeasonImages);
        MapCoversToLocal(resource);
        FetchAndLinkSeriesStatistics(resource);
        PopulateAlternateTitles(resource);
        PopulateAliases(resource);
        LinkRootFolderPath(resource);

        return resource;
    }

    private void MapCoversToLocal(params SeriesResource[] series)
    {
        foreach (var seriesResource in series)
        {
            _coverMapper.ConvertToLocalUrls(seriesResource.Id, seriesResource.Images, seriesResource.Added);
        }
    }

    private void FetchAndLinkSeriesStatistics(SeriesResource resource)
    {
        LinkSeriesStatistics(resource, _seriesStatisticsService.SeriesStatistics(resource.Id, resource.QualityProfileId));
    }

    private void LinkSeriesStatistics(List<SeriesResource> resources, Dictionary<int, SeriesStatistics> seriesStatistics)
    {
        foreach (var series in resources)
        {
            if (seriesStatistics.TryGetValue(series.Id, out var stats))
            {
                LinkSeriesStatistics(series, stats);
            }
        }
    }

    private void LinkSeriesStatistics(SeriesResource resource, SeriesStatistics seriesStatistics)
    {
        // Only set last aired from statistics if it's missing from the series itself
        resource.LastAired ??= seriesStatistics.LastAired;

        resource.PreviousAiring = seriesStatistics.PreviousAiring;
        resource.NextAiring = seriesStatistics.NextAiring;
        resource.Statistics = seriesStatistics.ToResource(resource.Seasons);

        if (seriesStatistics.SeasonStatistics != null)
        {
            foreach (var season in resource.Seasons)
            {
                season.Statistics = seriesStatistics.SeasonStatistics?.SingleOrDefault(s => s.SeasonNumber == season.SeasonNumber)?.ToResource();
            }
        }
    }

    private void PopulateAlternateTitles(List<SeriesResource> resources)
    {
        foreach (var resource in resources)
        {
            PopulateAlternateTitles(resource);
        }
    }

    private void PopulateAlternateTitles(SeriesResource resource)
    {
        var mappings = _sceneMappingService.FindByTvdbId(resource.TvdbId);

        if (mappings == null)
        {
            return;
        }

        // User defined aliases are injected into the scene mapping cache, they are exposed as Aliases instead.
        resource.AlternateTitles = mappings.Where(m => m.Type != SceneMapping.SeriesAliasType)
                                           .Select(AlternateTitleResourceMapper.ToResource)
                                           .ToList();
    }

    private void PopulateAliases(List<SeriesResource> resources)
    {
        var aliases = _seriesAliasService.GetAllTitlesBySeriesId();

        foreach (var resource in resources)
        {
            resource.Aliases = aliases.TryGetValue(resource.Id, out var titles) ? titles : new List<string>();
        }
    }

    private void PopulateAliases(SeriesResource resource)
    {
        resource.Aliases = _seriesAliasService.GetBySeriesId(resource.Id).Select(a => a.Title).ToList();
    }

    private bool IsAliasUsedByOtherSeries(SeriesResource resource, string? alias)
    {
        if (alias.IsNullOrWhiteSpace())
        {
            return false;
        }

        try
        {
            var existingSeries = _seriesService.FindByTitle(alias);

            if (existingSeries != null && existingSeries.Id != resource.Id)
            {
                return true;
            }

            var tvdbId = _sceneMappingService.FindTvdbId(alias, null, -1);

            return tvdbId.HasValue && tvdbId.Value != resource.TvdbId;
        }
        catch (MultipleSeriesFoundException)
        {
            return true;
        }
        catch (InvalidSceneMappingException)
        {
            return true;
        }
    }

    private bool IsAliasAlreadyKnownForSeries(SeriesResource resource, string? alias)
    {
        if (alias.IsNullOrWhiteSpace())
        {
            return false;
        }

        var cleanAlias = alias.CleanSeriesTitle();

        if (cleanAlias == resource.Title?.CleanSeriesTitle())
        {
            return true;
        }

        var mappings = _sceneMappingService.FindByTvdbId(resource.TvdbId);

        return mappings != null &&
               mappings.Any(m => m.Type != SceneMapping.SeriesAliasType && m.ParseTerm == cleanAlias);
    }

    private void LinkRootFolderPath(SeriesResource resource)
    {
        resource.RootFolderPath = _rootFolderService.GetBestRootFolderPath(resource.Path);
    }

    [NonAction]
    public void Handle(EpisodeImportedEvent message)
    {
        BroadcastResourceChange(ModelAction.Updated, message.ImportedEpisode.SeriesId);
    }

    [NonAction]
    public void Handle(EpisodeFileDeletedEvent message)
    {
        if (message.Reason == DeleteMediaFileReason.Upgrade)
        {
            return;
        }

        BroadcastResourceChange(ModelAction.Updated, message.EpisodeFile.SeriesId);
    }

    [NonAction]
    public void Handle(SeriesUpdatedEvent message)
    {
        BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
    }

    [NonAction]
    public void Handle(SeriesEditedEvent message)
    {
        var resource = GetSeriesResource(message.Series, false);

        if (resource == null)
        {
            return;
        }

        resource.EpisodesChanged = message.EpisodesChanged;
        BroadcastResourceChange(ModelAction.Updated, resource);
    }

    [NonAction]
    public void Handle(SeriesDeletedEvent message)
    {
        foreach (var series in message.Series)
        {
            var resource = GetSeriesResource(series, false);

            if (resource == null)
            {
                continue;
            }

            BroadcastResourceChange(ModelAction.Deleted, resource);
        }
    }

    [NonAction]
    public void Handle(SeriesRenamedEvent message)
    {
        BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
    }

    [NonAction]
    public void Handle(SeriesBulkEditedEvent message)
    {
        foreach (var series in message.Series)
        {
            var resource = GetSeriesResource(series, false);

            if (resource == null)
            {
                continue;
            }

            BroadcastResourceChange(ModelAction.Updated, resource);
        }
    }

    [NonAction]
    public void Handle(MediaCoversUpdatedEvent message)
    {
        if (message.Updated)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Series.Id);
        }
    }
}
