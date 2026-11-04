import type { EntityDto } from '@abp/ng.core';

export interface CreateUserDto {
  displayName: string;
  email: string;
  role: string;
}

export interface UpdateUserDto {
  displayName: string;
  email: string;
  role: string;
}

export interface UserDto extends EntityDto<string> {
  displayName?: string;
  email?: string;
  role?: string;
}
