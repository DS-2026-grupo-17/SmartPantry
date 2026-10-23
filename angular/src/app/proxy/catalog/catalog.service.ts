import type { GetProductByBarcodeDto, ProductLookupResultDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CatalogService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  getByBarcode = (input: GetProductByBarcodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductLookupResultDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/catalog/by-barcode',
      params: { barcode: input.barcode },
    },
    { apiName: this.apiName,...config });
}