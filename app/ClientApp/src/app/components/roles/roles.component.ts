import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {MatTableDataSource} from '@angular/material/table';
import {MatDialog} from '@angular/material/dialog';
import {RoleResponse, CreateRole, UpdateRole, RoleResponseDto} from "../../core/models/role.interface";
import {
    EntitySelectorDialogComponent
} from "../../shared/components/entity-display-dialog/entity-display-dialog.component";
import {RoleService} from "../../core/services/role.service";
import {merge, Subject, of, Observable} from "rxjs";
import {debounceTime, distinctUntilChanged, startWith, switchMap, filter, catchError} from "rxjs/operators";
import {QueryParams} from "../../core/models/query-params.interface";
import {MatPaginator} from "@angular/material/paginator";
import {MatSort} from "@angular/material/sort";
import {QueryService} from "../../core/services/query.service";
import {UserResponse} from "../../core/models/user.interface";
import {UserService} from "../../core/services/user.service";
import {ConfirmDialogComponent} from "../../shared/components/confirm-dialog/confirm-dialog.component";
import {BaseDialogComponent} from "../../shared/components/base-dialog/base-dialog.component";
import {DialogField} from 'src/app/shared/components/base-dialog/base-dialog.component';
import {Validators} from "@angular/forms";
import {PermissionService} from '../../core/services/permission.service';
import {Constants, DefaultValues} from 'src/app/core/constants';
import {NotificationUtils} from '../../core/utils/notification.utils';
import {FormUtils} from '../../core/utils/form.utils';

interface DialogConfig {
    service: () => Observable<any>;
    mapData: (item: any) => any;
    columns: { id: string; label: string }[];
}

@Component({
    selector: 'app-roles',
    templateUrl: './roles.component.html',
    styleUrls: ['./roles.component.scss']
})
export class RolesComponent implements OnInit, AfterViewInit {
    public readonly displayedColumns: string[] = ['name', 'createdAt', 'permissions', 'users', 'actions'];
    public readonly dataSource: MatTableDataSource<RoleResponse> = new MatTableDataSource();
    public searchTerm: Subject<string> = new Subject<string>();
    public isLoading: boolean = false;
    public totalItems: number = 0;
    public currentSearchValue: string = '';

    @ViewChild(MatPaginator) private readonly paginator!: MatPaginator;
    @ViewChild(MatSort) private readonly sort!: MatSort;

    private readonly roleFields: DialogField[] = [
        {
            name: 'name',
            type: 'text',
            label: 'Role Name',
            required: true,
            validators: [Validators.minLength(3)]
        },
        {
            name: 'permissionIds',
            type: 'multiselect',
            label: 'Permissions',
            required: true,
            options: [] // TODO: Add permissions options
        }
    ];

    constructor(private readonly dialog: MatDialog,
                private readonly roleService: RoleService,
                private readonly queryService: QueryService,
                private readonly permissionService: PermissionService,
                private readonly userService: UserService) {
    }

    ngOnInit(): void {
    }

    ngAfterViewInit(): void {
        this.loadRoles();
    }

    public loadRoles(): void {
        this.setupSearchAndSort();
    }

    public onSearch(event: Event): void {
        const target = event.target as HTMLInputElement;
        this.currentSearchValue = target.value;
        this.searchTerm.next(this.currentSearchValue);
    }

    public onCreate(): void {
        const dialogRef = this.dialog.open(BaseDialogComponent<CreateRole>, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                title: 'Create Role',
                fields: this.roleFields,
                mode: 'create'
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter((result: CreateRole) => !!result),
                switchMap((result: CreateRole) => this.roleService.createRole(result))
            )
            .subscribe({
                next: () => {
                    this.loadRoles();
                    NotificationUtils.showSuccess(this.dialog, 'Role has been created successfully');
                },
                error: (error) => {
                    console.error('Error creating role:', error);
                    NotificationUtils.showError(this.dialog, 'Failed to create role', error);
                }
            });
    }

    public onEdit(role: RoleResponse): void {
        const dialogRef = this.dialog.open(BaseDialogComponent<UpdateRole>, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                title: 'Edit Role',
                fields: this.roleFields,
                mode: 'edit',
                entity: {
                    id: role.id,
                    name: role.name,
                    permissionIds: role.permissionIds
                }
            }
        });

        const updateRole = (result: UpdateRole) => {
            return this.roleService.updateRole({
                id: role.id,
                name: result.name,
                permissionIds: result.permissionIds
            });
        };

        dialogRef.afterClosed()
            .pipe(
                filter((result: UpdateRole) => !!result),
                switchMap(updateRole)
            )
            .subscribe({
                next: () => {
                    this.loadRoles();
                    NotificationUtils.showSuccess(this.dialog, `Role "${role.name}" has been updated successfully`);
                },
                error: (error) => {
                    console.error('Error updating role:', error);
                    NotificationUtils.showError(this.dialog, 'Failed to update role', error);
                }
            });
    }

    public onDelete(role: RoleResponse): void {
        const dialogRef = this.dialog.open(ConfirmDialogComponent, {
            width: Constants.DIALOG_WIDTH,
            data: {
                title: 'Confirm Delete',
                message: `Are you sure you want to delete role "${role.name}"?`,
                confirmButton: 'Delete',
                cancelButton: 'Cancel'
            }
        });

        dialogRef.afterClosed().subscribe((result: boolean) => {
            if (result) {
                this.roleService.deleteRole(role.id)
                    .pipe(
                        catchError(error => {
                            NotificationUtils.showError(this.dialog, `Failed to delete role ${role.name}`, error);
                            return of(null);
                        })
                    )
                    .subscribe(() => {
                        this.loadRoles();
                        NotificationUtils.showSuccess(this.dialog, `Role "${role.name}" has been deleted successfully`);
                    });
            }
        });
    }

    public openEntityDialog(type: 'users' | 'permissions', ids: number[]): void {
        if (ids.length === 0) {
            NotificationUtils.showInfo(this.dialog, `No ${type} assigned to this role`);
            return;
        }

        const dialogConfig: { [key in 'users' | 'permissions']: DialogConfig } = {
            users: {
                service: () => this.userService.searchUsers({userIds: ids}),
                mapData: (user: UserResponse) => ({
                    id: user.id,
                    name: user.username,
                    email: user.email
                }),
                columns: [
                    {id: 'name', label: 'Name'},
                    {id: 'email', label: 'Email'}
                ]
            },
            permissions: {
                service: () => this.permissionService.searchPermissions({permissionIds: ids}),
                mapData: (permission: any) => ({
                    id: permission.id,
                    name: permission.name,
                    description: permission.description,
                    permissionString: this.permissionService.permissionStringToReadable(permission.permissionString)
                }),
                columns: [
                    {id: 'name', label: 'Name'},
                    {id: 'description', label: 'Description'},
                    {id: 'permissionString', label: 'Permission String'}
                ]
            }
        };

        const config = dialogConfig[type];
        config.service().subscribe({
            next: (response: any) => {
                const data = response.items.map(config.mapData);
                this.openEntitySelectorDialog(data, config.columns);
            },
            error: (error: Error) => {
                console.error(`Error fetching ${type}:`, error);
                NotificationUtils.showError(this.dialog, `Failed to fetch ${type}`, error);
            }
        });
    }

    private openEntitySelectorDialog(entities: any[], columns: { id: string, label: string }[]): void {
        this.dialog.open(EntitySelectorDialogComponent, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                entities,
                columns
            }
        });
    }

    private setupSearchAndSort(): void {
        const permissionsField = this.roleFields.find(field => field.name === 'permissionIds');
        if (!permissionsField) {
            console.error('Permissions field not found');
            return;
        }

        this.permissionService.getPermissions(DefaultValues.QUERY_PARAMS)
            .subscribe(permissions => {
                permissionsField.options = FormUtils.toOptions(permissions.items);
            });

        merge(
            this.searchTerm.pipe(
                debounceTime(300),
                distinctUntilChanged(),
                filter(term => !term || term.length >= 3)
            ),
            this.sort.sortChange,
            this.paginator.page
        )
            .pipe(
                startWith({}),
                switchMap(() => {
                    this.isLoading = true;
                    const params: QueryParams = {
                        page: this.paginator.pageIndex + 1,
                        pageSize: this.paginator.pageSize,
                        searchTerm: this.currentSearchValue,
                        sortColumn: this.sort.active,
                        sortDirection: this.sort.direction as 'asc' | 'desc'
                    };

                    return this.currentSearchValue && this.currentSearchValue.length >= 3
                        ? this.roleService.findRolesByTerm(this.currentSearchValue)
                        : this.roleService.getRoles(this.queryService.normaliseQueryParams(params));
                }),
                catchError((error: Error) => {
                    console.error('Error fetching roles:', error);
                    NotificationUtils.showError(this.dialog, 'Failed to fetch roles', error);
                    return of<RoleResponseDto>({items: [], total: 0, page: 1, pageSize: 50});
                })
            )
            .subscribe({
                next: (response: RoleResponseDto) => {
                    this.dataSource.data = response.items;
                    this.totalItems = response.total;
                    this.isLoading = false;
                }
            });
    }

}
