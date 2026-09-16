# <img width="24px" src="./Logo/256.png" alt="Sonarr Extended"></img> Sonarr Extended

Sonarr Extended is an unofficial fork of [Sonarr](https://github.com/Sonarr/Sonarr), the PVR for Usenet and BitTorrent users. It follows Sonarr's development branches closely and adds features that are not available upstream.

> [!IMPORTANT]
> **Sonarr Extended is not affiliated with, endorsed by, or supported by the Sonarr team.**
> Do not report issues encountered with Sonarr Extended to Sonarr's GitHub, forums, Discord or Reddit. Open them [here](https://github.com/Developpeur-du-dimanche/sonarr-extended/issues) instead.

> [!WARNING]
> **Switching from Sonarr to Sonarr Extended is a one-way migration.**
>
> Sonarr Extended adds its own database migrations (for example new columns on the `Series` table) on top of Sonarr's. Once Sonarr Extended has started on your database:
>
> - **Going back to official Sonarr with the same database is not supported.** Sonarr uses sequential migration numbers, so its future migrations may share numbers with ones Sonarr Extended already applied. They would be considered as already run and silently skipped, leaving the database in a broken or inconsistent state.
> - Episode data refreshed from an alternative metadata source or episode order may no longer match what Sonarr expects.
> - Sonarr's built-in updater must not be used to switch back: update Sonarr Extended only from [this repository's releases](https://github.com/Developpeur-du-dimanche/sonarr-extended/releases).
>
> **Back up your Sonarr data folder (including `sonarr.db`) before switching.** Restoring that backup with official Sonarr is the only supported way back.

## Additional Features

- **TMDB metadata source**: use [TMDB](https://www.themoviedb.org/) episode data as an alternative to TVDB for a series
- **Episode orders**: pick a TMDB episode group per series to use an alternative episode ordering (absolute, DVD, story arcs, …)

Everything else comes from Sonarr: see [Sonarr's features](https://github.com/Sonarr/Sonarr#features).

## Getting Started

- [Download/Installation](https://github.com/Developpeur-du-dimanche/sonarr-extended/releases)
- [Bugs and Feature Requests](https://github.com/Developpeur-du-dimanche/sonarr-extended/issues)

Most of Sonarr's documentation applies to Sonarr Extended as well:

- [FAQ](https://wiki.servarr.com/sonarr/faq)
- [Wiki](https://wiki.servarr.com/Sonarr)
- [API Documentation](https://sonarr.tv/docs/api)

## Relationship with Sonarr

| Sonarr Extended branch | Follows Sonarr branch |
| ---------------------- | --------------------- |
| `v5-develop`           | [`v5-develop`](https://github.com/Sonarr/Sonarr/tree/v5-develop) |
| `develop`              | [`develop`](https://github.com/Sonarr/Sonarr/tree/develop) |

Changes from Sonarr are merged automatically through pull requests opened by the [Sync Upstream](.github/workflows/sync_upstream.yml) workflow.

## Credits

Sonarr Extended would not exist without [Sonarr](https://sonarr.tv) and all of [its contributors](https://github.com/Sonarr/Sonarr/graphs/contributors). All credit for the application itself goes to them.

If you find Sonarr useful, please consider [supporting the Sonarr project](https://sonarr.tv/donate).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Contributions that are not specific to Sonarr Extended should be submitted to [Sonarr](https://github.com/Sonarr/Sonarr) directly, so that everyone benefits from them.

## License

- [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
- Copyright 2010-2026 Sonarr contributors
- Modifications copyright 2026 Sonarr Extended contributors
