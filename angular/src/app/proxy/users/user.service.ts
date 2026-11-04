import type { CreateUserDto, UpdateUserDto, UserDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUserDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, UserDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/user',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/user/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, UserDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/user/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<UserDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/user',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateUserDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, UserDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/user/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}