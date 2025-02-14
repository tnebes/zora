import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {
    PermissionResponseDto,
    PermissionResponse,
    QueryParams,
    CreatePermission,
    UpdatePermission
} from '../models/permission.interface';
import {Constants} from '../constants';
import {QueryService} from "./query.service";

@Injectable({
    providedIn: 'root'
})
export class PermissionService {
    private readonly apiUrl: string = Constants.PERMISSIONS;

    constructor(
        private readonly http: HttpClient,
        private readonly queryService: QueryService
    ) {
    }

    public getPermissions(queryParams: QueryParams): Observable<PermissionResponseDto> {
        const params: HttpParams = this.queryService.getHttpParams(queryParams);
        return this.http.get<PermissionResponseDto>(this.apiUrl, {params});
    }

    public create(permission: CreatePermission): Observable<PermissionResponse> {
        return this.http.post<PermissionResponse>(this.apiUrl, permission);
    }

    public update(permission: UpdatePermission): Observable<PermissionResponse> {
        return this.http.put<PermissionResponse>(`${this.apiUrl}/${permission.id}`, permission);
    }

    public delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    public findPermissionsByTerm(searchTerm: string): Observable<PermissionResponseDto> {
        if (!searchTerm || searchTerm.length < 3) {
            console.error('Search term must be at least 3 characters long');
            throw new Error('Search term must be at least 3 characters long');
        }

        const params = new HttpParams().set('searchTerm', searchTerm);

        return this.http.get<PermissionResponseDto>(`${Constants.PERMISSIONS_FIND}`, {params});
    }

    public searchPermissions(params: { permissionIds: number[] }): Observable<PermissionResponseDto> {
        const httpParams = new HttpParams().set(Constants.ID, params.permissionIds.join(','));
        return this.http.get<PermissionResponseDto>(`${Constants.PERMISSIONS_SEARCH}`, {params: httpParams});
    }

    public permissionStringToReadable(permissionString: string): string[] {
        const permissions: string[] = [];
        for (let i = 0; i < permissionString.length; i++) {
            if (permissionString[i] === '1') {
                permissions.push(Constants.PERMISSION_STRING_BIT_NAMES[i]);
            }
        }
        return permissions;
    }
}
