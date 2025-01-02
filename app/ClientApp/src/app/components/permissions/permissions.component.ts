import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {MatPaginator} from '@angular/material/paginator';
import {MatDialog} from '@angular/material/dialog';
import {MatTableDataSource} from '@angular/material/table';
import {MatSort} from "@angular/material/sort";
import {Validators} from '@angular/forms';
import {Subject, catchError, of, merge as observableMerge} from 'rxjs';
import {debounceTime, distinctUntilChanged, filter, merge, startWith, switchMap} from 'rxjs/operators';
import {PermissionResponse, CreatePermission, UpdatePermission} from '../../core/models/permission.interface';
import {PermissionService} from '../../core/services/permission.service';
import {BaseDialogComponent, DialogField} from '../../shared/components/base-dialog/base-dialog.component';
import {NotificationDialogComponent} from "../../shared/components/notification-dialog/notification-dialog.component";
import {ConfirmDialogComponent} from "../../shared/components/confirm-dialog/confirm-dialog.component";
import {Constants} from '../../core/constants';
import {QueryParams} from '../../core/models/query-params.interface';
import {QueryService} from '../../core/services/query.service';
import {
    EntitySelectorDialogComponent
} from '../../shared/components/entity-display-dialog/entity-display-dialog.component';
import {RoleService} from '../../core/services/role.service';

@Component({
    selector: 'app-permissions',
    templateUrl: './permissions.component.html',
    styleUrls: ['./permissions.component.scss']
})
export class PermissionsComponent implements OnInit, AfterViewInit {
    public readonly displayedColumns: string[] = ['name', 'description', 'createdAt', 'roles', 'actions'];
    public readonly dataSource: MatTableDataSource<PermissionResponse> = new MatTableDataSource();
    public searchTerm: Subject<string> = new Subject<string>();
    public isLoading: boolean = false;
    public totalItems: number = 0;
    public currentSearchValue: string = '';

    @ViewChild(MatPaginator) private readonly paginator!: MatPaginator;
    @ViewChild(MatSort) private readonly sort!: MatSort;

    private readonly permissionFields: DialogField[] = [
        {
            name: 'name',
            type: 'text',
            label: 'Permission Name',
            required: true,
            validators: [Validators.minLength(3)]
        },
        {
            name: 'description',
            type: 'text',
            label: 'Description',
            required: false
        }
    ];

    constructor(
        private readonly dialog: MatDialog,
        private readonly permissionService: PermissionService,
        private readonly queryService: QueryService,
        private readonly roleService: RoleService
    ) {
    }

    ngOnInit(): void {
    }

    ngAfterViewInit(): void {
        this.loadPermissions();
    }

    public loadPermissions(): void {
        this.setupSearchAndSort();
    }

    public onSearch(event: Event): void {
        const target = event.target as HTMLInputElement;
        this.currentSearchValue = target.value;
        this.searchTerm.next(this.currentSearchValue);
    }

    public onCreate(): void {
        const dialogRef = this.dialog.open(BaseDialogComponent<CreatePermission>, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                title: 'Create Permission',
                fields: this.permissionFields,
                mode: 'create'
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter(result => !!result),
                switchMap(result => this.permissionService.create(result))
            )
            .subscribe({
                next: () => {
                    this.loadPermissions();
                    this.showNotification('Success', 'Permission has been created successfully');
                },
                error: (error) => {
                    console.error('Error creating permission:', error);
                    this.showNotification('Error', `Failed to create permission: ${error.message}`, 'warning');
                }
            });
    }

    public onEdit(permission: PermissionResponse): void {
        const dialogRef = this.dialog.open(BaseDialogComponent<UpdatePermission>, {
            width: Constants.ENTITY_DIALOG_WIDTH,
            data: {
                title: 'Edit Permission',
                fields: this.permissionFields,
                mode: 'edit',
                entity: {
                    id: permission.id,
                    name: permission.name,
                    description: permission.description,
                    permissionString: permission.permissionString
                }
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter(result => !!result),
                switchMap(result => this.permissionService.update({
                    id: permission.id,
                    name: result.name,
                    description: result.description,
                    permissionString: permission.permissionString
                }))
            )
            .subscribe({
                next: () => {
                    this.loadPermissions();
                    this.showNotification('Success', `Permission "${permission.name}" has been updated successfully`);
                },
                error: (error) => {
                    console.error('Error updating permission:', error);
                    this.showNotification('Error', `Failed to update permission: ${error.message}`, 'warning');
                }
            });
    }

    public onDelete(permission: PermissionResponse): void {
        const dialogRef = this.dialog.open(ConfirmDialogComponent, {
            width: Constants.DIALOG_WIDTH,
            data: {
                title: 'Confirm Delete',
                message: `Are you sure you want to delete permission "${permission.name}"?`,
                confirmButton: 'Delete',
                cancelButton: 'Cancel'
            }
        });

        dialogRef.afterClosed().subscribe((result: boolean) => {
            if (result) {
                this.permissionService.delete(permission.id)
                    .pipe(
                        catchError(error => {
                            this.showNotification('Error', `Failed to delete permission ${permission.name}: ${error.message}`, 'warning');
                            return of(null);
                        })
                    )
                    .subscribe(() => {
                        this.loadPermissions();
                        this.showNotification('Success', `Permission "${permission.name}" has been deleted successfully`);
                    });
            }
        });
    }

    public openEntityDialog(roleIds: number[]): void {
        if (roleIds.length === 0) {
            this.dialog.open(NotificationDialogComponent, {
                width: Constants.DIALOG_WIDTH,
                data: {
                    message: 'No roles assigned to this permission'
                }
            });
            return;
        }

        this.roleService.searchRoles({roleIds}).subscribe((roles) => {
            const data = roles.items.map(role => ({
                id: role.id,
                name: role.name
            }));
            this.openEntitySelectorDialog(data, [
                {id: 'name', label: 'Name'},
                {id: 'id', label: 'ID'}
            ]);
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
        observableMerge([
            this.searchTerm.pipe(
                debounceTime(300),
                distinctUntilChanged(),
                filter(term => !term || term.length >= 3)
            ),
            this.sort.sortChange,
            this.paginator.page
        ])
            .pipe(
                startWith({}),
                switchMap(() => {
                    this.isLoading = true;

                    if (!this.currentSearchValue || this.currentSearchValue.length < 3) {
                        const params: QueryParams = {
                            page: this.paginator.pageIndex + 1,
                            pageSize: this.paginator.pageSize,
                            searchTerm: '',
                            sortColumn: this.sort.active,
                            sortDirection: this.sort.direction as 'asc' | 'desc'
                        };
                        return this.permissionService.getPermissions(this.queryService.normaliseQueryParams(params));
                    }
                    return this.permissionService.findPermissionsByTerm(this.currentSearchValue);
                }),
                catchError(error => {
                    console.error('Error fetching permissions:', error);
                    this.showNotification('Error', 'Failed to fetch permissions', 'warning');
                    return of({items: [], total: 0});
                })
            )
            .subscribe(response => {
                this.dataSource.data = response.items;
                this.totalItems = response.total;
                this.isLoading = false;
            });
    }

    private showNotification(title: string, message: string, type: 'information' | 'warning' = 'information'): void {
        this.dialog.open(NotificationDialogComponent, {
            width: Constants.DIALOG_WIDTH,
            data: {
                title,
                message,
                type
            }
        });
    }
}
