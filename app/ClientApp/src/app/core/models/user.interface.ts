import {Response} from './response.interface';

export interface User {
  id: number;
  username: string;
  email: string;
  createdAt: Date;
  roles?: string[];
}

export interface UserResponse extends Response {
  items: User[];
}
