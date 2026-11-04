import { mapEnumToOptions } from '@abp/ng.core';

export enum ProductLookupStatus {
  Found = 'Found',
  NotFound = 'NotFound',
  RateLimited = 'RateLimited',
  Unavailable = 'Unavailable',
}

export const productLookupStatusOptions = mapEnumToOptions(ProductLookupStatus);
