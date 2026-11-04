import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreatePantryItemDto extends PantryItemDataDto {
  productId: string;
}

export interface GetPantryItemsInput extends PagedAndSortedResultRequestDto {
  includeConsumed?: boolean;
}

export interface PantryItemDataDto {
  quantity?: number | null;
  unit?: string | null;
  expirationDate?: string | null;
}

export interface PantryItemDto extends EntityDto<string> {
  productId?: string;
  productName?: string;
  productBarcode?: string;
  quantity?: number | null;
  unit?: string | null;
  expirationDate?: string | null;
  isConsumed?: boolean;
}

export interface UpdatePantryItemDto extends PantryItemDataDto {
}
