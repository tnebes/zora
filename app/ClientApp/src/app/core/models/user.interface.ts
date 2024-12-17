export interface User {
  id: number;
  username: string;
  email: string;
  createdAt: Date;
  roles?: string[];
}

export interface Response {
  items: Object[];
  total: number;
  page: number;
  pageSize: number;
}

export interface UserResponse extends Response {
  items: User[];
}

export interface UserQueryParams {
  page: number;
  pageSize: number;
  searchTerm?: string;
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
}
