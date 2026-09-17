import React from 'react';
import Modal from 'Components/Modal/Modal';
import SeriesAliasesModalContent, {
  SeriesAliasesModalContentProps,
} from './SeriesAliasesModalContent';

interface SeriesAliasesModalProps extends SeriesAliasesModalContentProps {
  isOpen: boolean;
}

function SeriesAliasesModal({
  isOpen,
  onModalClose,
  ...otherProps
}: SeriesAliasesModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SeriesAliasesModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default SeriesAliasesModal;
