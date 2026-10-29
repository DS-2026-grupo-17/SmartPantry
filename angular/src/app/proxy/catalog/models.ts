import type { ProductLookupStatus } from './product-lookup-status.enum';

export interface CatalogProductDto {
  barcode?: string;
  name?: string | null;
  brand?: string | null;
  imageUrl?: string | null;
  nutriScore?: string | null;
  novaGroup?: number | null;
  allergens?: string[] | null;
}

export interface GetProductByBarcodeDto {
  barcode: string;
}

export interface ProductLookupResultDto {
  status?: ProductLookupStatus;
  barcode?: string;
  product?: CatalogProductDto | null;
}
