# TranslationBatchSignal

Wakes the batch loop early. A bounded channel of one slot that drops extra writes: any
number of notifications while the loop is busy collapse into one more round, never a queue.
