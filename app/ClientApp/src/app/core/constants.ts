import {QueryParams} from "./models/query-params.interface";

export class Constants {
    static readonly BASE_URL: string = "https://localhost:5001/api/v1"; // TODO this should be an environment variable & should be accessible via proxy
    static readonly AUTHENTICATION: string = `${Constants.BASE_URL}/authentication`;
    static readonly TOKEN: string = `${Constants.AUTHENTICATION}/token`;
    static readonly CURRENT_USER: string = `${Constants.AUTHENTICATION}/current-user`;
    static readonly REGISTRATION: string = `${Constants.BASE_URL}/registration`;
    static readonly AUTHORISATION: string = `${Constants.BASE_URL}/authorisation`;
    static readonly IS_ADMIN: string = `${Constants.AUTHORISATION}/is-admin`;
    static readonly AUTHENTICATION_CHECK: string = `${Constants.AUTHENTICATION}/check`;
    static readonly JWT_TOKEN_KEY: string = "zora_jwt_token";

    static readonly USERS: string = `${Constants.BASE_URL}/users`;
    static readonly USERS_SEARCH: string = `${Constants.USERS}/search`;
    static readonly ROLES: string = `${Constants.BASE_URL}/roles`;
    static readonly MAX_PAGE_SIZE: number = 1000;

    static readonly ID: string = 'id';
    static readonly USERNAME: string = 'username';
    static readonly EMAIL: string = 'email';
    static readonly ROLE: string = 'role';
    static readonly PERMISSION: string = 'permission';
    static readonly CREATED_AT: string = 'createdAt';
    static readonly ROLE_NAME: string = 'roleName';
    static readonly USERS_FIND: string = `${Constants.USERS}/find`;
    static readonly ROLES_FIND: string = `${Constants.ROLES}/find`;
    static readonly DIALOG_WIDTH: string = '400px';
    static readonly ENTITY_DIALOG_WIDTH: string = '600px';

}

export class DefaultValues {
    static readonly QUERY_PARAMS: QueryParams = {
        page: 1,
        pageSize: 50,
        searchTerm: '',
        sortColumn: 'createdAt',
        sortDirection: 'asc'
    };
}
