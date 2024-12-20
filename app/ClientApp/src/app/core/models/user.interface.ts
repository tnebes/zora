import {Response} from './response.interface';

export interface AbstractUser {
  id: number;
  username: string;
  email: string;
}

export interface User extends AbstractUser {
  createdAt: Date;
  roles?: string[];
}

export interface CreateUser extends AbstractUser {
  password: string;
  roles?: number[];
}

export interface UpdateUser extends AbstractUser {
  roles?: number[];
}

export interface UserResponse extends Response {
  items: User[];
}
