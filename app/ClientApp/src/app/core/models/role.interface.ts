import {ResponseDto} from './response-dto.interface';

export interface RoleResponseDto extends ResponseDto {
    items: RoleResponse[];
}

export interface AbstractRole {
    id: number;
    name: string;
}

export interface RoleResponse extends AbstractRole {
    createdAt: Date;
    userIds: number[];
    permissionIds: number[];
}

export interface CreateRole {
    name: string;
    permissionIds: number[];
}

export interface UpdateRole {
    id: number;
    name: string;
    permissionIds: number[];
}
