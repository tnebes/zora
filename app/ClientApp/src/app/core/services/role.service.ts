import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {Constants} from '../constants';
import {RoleResponseDto} from "../models/role.interface";
import {QueryParams} from "../models/query-params.interface";
import {QueryService} from "./query.service";

@Injectable({
    providedIn: 'root'
})
export class RoleService {
    private readonly apiUrl: string = Constants.ROLES;

    constructor(private readonly http: HttpClient, private readonly queryService: QueryService) {
    }

    public getRoles(queryParams : QueryParams): Observable<RoleResponseDto> {
        const params : HttpParams = this.queryService.getHttpParams(queryParams);
        return this.http.get<RoleResponseDto>(this.apiUrl, {params});
    }
}
