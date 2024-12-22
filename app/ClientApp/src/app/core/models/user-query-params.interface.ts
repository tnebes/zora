export interface UserQueryParams {
    page: number;
    pageSize: number;
    searchTerm?: string;
    sortColumn?: string;
    sortDirection?: 'asc' | 'desc';
}
