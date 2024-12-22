import {ResponseDto} from './responseDto.interface';

export interface RoleResponse extends ResponseDto {
    items: Role[];
}

export interface AbstractRole {
    id: number;
    name: string;
}

export interface Role extends AbstractRole {
    createdAt: Date;
    userIds: number[];
    permissionIds: number[];
}
