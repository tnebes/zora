import { environment } from "src/environments/environment";
import {QueryParams} from "./models/query-params.interface";

export class Constants {
    // Base URL
    static readonly BASE_URL: string = `https://localhost:${environment.backendPort}/api/v1`;

    // Authentication & Authorization
    static readonly AUTHENTICATION: string = `${Constants.BASE_URL}/authentication`;
    static readonly AUTHORISATION: string = `${Constants.BASE_URL}/authorisation`;
    static readonly TOKEN: string = `${Constants.AUTHENTICATION}/token`;
    static readonly CURRENT_USER: string = `${Constants.AUTHENTICATION}/current-user`;
    static readonly AUTHENTICATION_CHECK: string = `${Constants.AUTHENTICATION}/check`;
    static readonly IS_ADMIN: string = `${Constants.AUTHORISATION}/is-admin`;
    static readonly JWT_TOKEN_KEY: string = "zora_jwt_token";
    static readonly REGISTRATION: string = `${Constants.BASE_URL}/registration`;

    // Users endpoints
    static readonly USERS: string = `${Constants.BASE_URL}/users`;
    static readonly USERS_FIND: string = `${Constants.USERS}/find`;
    static readonly USERS_SEARCH: string = `${Constants.USERS}/search`;

    // Roles endpoints
    static readonly ROLES: string = `${Constants.BASE_URL}/roles`;
    static readonly ROLES_FIND: string = `${Constants.ROLES}/find`;
    static readonly ROLES_SEARCH: string = `${Constants.ROLES}/search`;

    // Permissions endpoints
    static readonly PERMISSIONS: string = `${Constants.BASE_URL}/permissions`;
    static readonly PERMISSIONS_FIND: string = `${Constants.PERMISSIONS}/find`;
    static readonly PERMISSIONS_SEARCH: string = `${Constants.PERMISSIONS}/search`;

    // Assets endpoints
    static readonly ASSETS: string = `${Constants.BASE_URL}/assets`;
    static readonly ASSETS_FIND: string = `${Constants.ASSETS}/find`;
    static readonly ASSETS_SEARCH: string = `${Constants.ASSETS}/search`;

    // Common field names
    static readonly ID: string = 'id';
    static readonly USERNAME: string = 'username';
    static readonly EMAIL: string = 'email';
    static readonly ROLE: string = 'role';
    static readonly PERMISSION: string = 'permission';
    static readonly CREATED_AT: string = 'createdAt';
    static readonly ROLE_NAME: string = 'roleName';
    static readonly NAME: string = 'name';
    static readonly USER: string = 'user';

    // UI Constants
    static readonly DIALOG_WIDTH: string = '400px';
    static readonly ENTITY_DIALOG_WIDTH: string = '800px';
    static readonly MAX_PAGE_SIZE: number = 1000;

    // Permission constants
    static readonly PERMISSION_STRING_BITS: number = 5;
    static readonly PERMISSION_STRING_BIT_NAMES: string[] = ['Manage', 'Delete', 'Create', 'Write', 'Read'];

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
