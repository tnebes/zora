import {ResponseDto} from './response-dto.interface';

export interface PermissionResponseDto extends ResponseDto {
    items: PermissionResponse[];
}

export interface AbstractPermission {
    id: number;
    name: string;
}

export interface PermissionResponse extends AbstractPermission {
    description: string;
    permissionString: string;
    createdAt: Date;
    deleted: boolean;
    roleIds: number[];
    workItemIds?: number[];
}

export interface CreatePermission {
    name: string;
    description: string;
    permissionString: string;
    workItemIds: number[];
}

export interface UpdatePermission {
    id: number;
    name: string;
    description: string;
    permissionString: string;
    workItemIds: number[];
}

export interface QueryParams {
    page: number;
    pageSize: number;
    searchTerm?: string;
    roleIds?: number[];
    workItemIds?: number[];
}
