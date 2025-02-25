import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {CreateUser, UpdateUser, User, UserResponse, UserResponseDto} from '../models/user.interface';
import {QueryParams} from '../models/query-params.interface';
import {Constants} from '../constants';
import {QueryService} from "./query.service";

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private readonly apiUrl: string = Constants.USERS;
    private readonly apiSearchUrl = Constants.USERS_SEARCH;

    constructor(private readonly http: HttpClient, private readonly queryService: QueryService) {
    }

    public getUserById(id: number): Observable<UserResponse> {
        return this.http.get<UserResponse>(`${this.apiUrl}/${id}`);
    }

    public getUsers(params: QueryParams): Observable<UserResponseDto<UserResponse>> {
        let httpParams: HttpParams = this.queryService.getHttpParams(params);
        return this.http.get<UserResponseDto<UserResponse>>(this.apiUrl, {params: httpParams});
    }

    public deleteUser(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    public createUser(user: CreateUser): Observable<User> {
        return this.http.post<User>(this.apiUrl, user);
    }

    public updateUser(user: UpdateUser): Observable<User> {
        return this.http.put<User>(`${this.apiUrl}/${user.id}`, user);
    }

    // TODO FIXME WTF
    // this method SHOULD take in a Map<number, string> but it's taking in an Object instead
    // because the API / dialog is returning an Object
    // so now we have to convert it back to a Map
    // this is a mess
    public formatRolesForSelection(roles: Object): number[] {
        return Object.keys(roles).map(key => parseInt(key));
    }

    public searchUsers(
        params: {
            userIds?: number[];
            usernames?: string[];
            emails?: string[];
            roles?: number[];
            roleNames?: string[];
            permissions?: number[];
            createdAt?: Date;
        }
    ): Observable<UserResponseDto<UserResponse>> {
        if (!Object.values(params).some(value => Array.isArray(value) ? value.length > 0 : value)) {
            throw new Error('At least one parameter must be provided.');
        }

        const queryParams: { [key: string]: string } = {
            ...(params.userIds?.length && {[Constants.ID]: params.userIds.join(',')}),
            ...(params.usernames?.length && {[Constants.USERNAME]: params.usernames.join(',')}),
            ...(params.emails?.length && {[Constants.EMAIL]: params.emails.join(',')}),
            ...(params.roles?.length && {[Constants.ROLE]: params.roles.join(',')}),
            ...(params.roleNames?.length && {[Constants.ROLE_NAME]: params.roleNames.join(',')}),
            ...(params.permissions?.length && {[Constants.PERMISSION]: params.permissions.join(',')}),
            ...(params.createdAt && {[Constants.CREATED_AT]: params.createdAt.toISOString()}),
        };

        const httpParams = new HttpParams({fromObject: queryParams});

        return this.http.get<UserResponseDto<UserResponse>>(this.apiSearchUrl, {params: httpParams});
    }

    public searchUsersByTerm(searchTerm: string): Observable<UserResponseDto<UserResponse>> {
        if (!searchTerm || searchTerm.length < 3) {
            console.error('Search term must be at least 3 characters long');
            throw new Error('Search term must be at least 3 characters long');
        }

        const params = new HttpParams()
            .set('searchTerm', searchTerm);

        return this.http.get<UserResponseDto<UserResponse>>(Constants.USERS_FIND, {params});
    }
}
