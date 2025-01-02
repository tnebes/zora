import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {Constants} from '../constants';
import {CreateRole, RoleResponse, RoleResponseDto, UpdateRole} from "../models/role.interface";
import {QueryParams} from "../models/query-params.interface";
import {QueryService} from "./query.service";

@Injectable({
    providedIn: 'root'
})
export class RoleService {

    private readonly apiUrl: string = Constants.ROLES;

    constructor(private readonly http: HttpClient, private readonly queryService: QueryService) {
    }

    public getRoles(queryParams: QueryParams): Observable<RoleResponseDto> {
        const params: HttpParams = this.queryService.getHttpParams(queryParams);
        return this.http.get<RoleResponseDto>(this.apiUrl, {params});
    }

    public createRole(role: CreateRole): Observable<RoleResponse> {
        return this.http.post<RoleResponse>(this.apiUrl, role);
    }

    public updateRole(role: UpdateRole): Observable<RoleResponse> {
        return this.http.put<RoleResponse>(`${this.apiUrl}/${role.id}`, role);
    }

    public deleteRole(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    public findRolesByTerm(searchTerm: string): Observable<RoleResponseDto> {
        if (!searchTerm || searchTerm.length < 3) {
            console.error('Search term must be at least 3 characters long');
            throw new Error('Search term must be at least 3 characters long');
        }

        const params = new HttpParams()
            .set('searchTerm', searchTerm);

        return this.http.get<RoleResponseDto>(`${Constants.ROLES_FIND}`, {params});
    }

    public searchRoles(params: { roleIds: number[] }): Observable<RoleResponseDto> {
        const httpParams = new HttpParams().set('roleIds', params.roleIds.join(','));
        return this.http.get<RoleResponseDto>(`${Constants.ROLES_SEARCH}`, {params: httpParams});
    }
}
