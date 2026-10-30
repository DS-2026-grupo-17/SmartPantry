import type { EntityDto } from '@abp/ng.core';

export interface ProductDto extends EntityDto<string> {
  barcode?: string;
  name?: string;
}

export interface SaveProductDto {
  barcode: string;
  name: string;
}
