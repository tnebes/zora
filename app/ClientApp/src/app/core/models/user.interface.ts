import {ResponseDto} from './response-dto.interface';
import {RoleResponse} from "./role.interface";

export interface AbstractUser {
    id: number;
    username: string;
    email: string;
}

export interface User extends AbstractUser {
    createdAt: Date;
    roles: RoleResponse[]
}

export interface CreateUser extends AbstractUser {
    password: string;
    roles?: number[];
}

export interface UpdateUser extends AbstractUser {
    roleIds: number[];
}

export interface UserResponse extends AbstractUser {
    createdAt: Date;
    roles: { [key: number]: string };
}

export interface UserResponseDto<T> extends ResponseDto {
    items: T[];
}
