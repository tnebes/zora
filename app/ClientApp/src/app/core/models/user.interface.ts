import {ResponseDto} from './responseDto.interface';
import {AbstractRole, Role} from "./role.interface";

export interface AbstractUser {
    id: number;
    username: string;
    email: string;
}

export interface User extends AbstractUser {
    createdAt: Date;
    roles: Role[]
}

export interface CreateUser extends AbstractUser {
    password: string;
    roles?: number[];
}

export interface UpdateUser extends AbstractUser {
    roles: number[];
}

export interface UserResponse extends AbstractUser {
    createdAt: Date;
    roles: Map<number, string>;
}

export interface UserResponseDto<T> extends ResponseDto {
    items: T[];
}
