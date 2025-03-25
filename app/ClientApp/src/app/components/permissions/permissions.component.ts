import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {MatPaginator} from '@angular/material/paginator';
import {MatDialog} from '@angular/material/dialog';
import {MatTableDataSource} from '@angular/material/table';
import {MatSort} from "@angular/material/sort";
import {Validators} from '@angular/forms';
import {Subject, catchError, of, merge} from 'rxjs';
import {debounceTime, distinctUntilChanged, filter, startWith, switchMap} from 'rxjs/operators';
import {PermissionResponse, CreatePermission, UpdatePermission} from '../../core/models/permission.interface';
import {PermissionService} from '../../core/services/permission.service';
import {BaseDialogComponent, DialogField} from '../../shared/components/base-dialog/base-dialog.component';
import {ConfirmDialogComponent} from "../../shared/components/confirm-dialog/confirm-dialog.component";
import {Constants} from '../../core/constants';
import {QueryParams} from '../../core/models/query-params.interface';
import {QueryService} from '../../core/services/query.service';
import {
    EntitySelectorDialogComponent
} from '../../shared/components/entity-display-dialog/entity-display-dialog.component';
import {RoleService} from '../../core/services/role.service';
import {NotificationUtils} from '../../core/utils/notification.utils';

interface PermissionFlagOption {
    value: number;
    display: string;
    mask: string;
}

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
    public errorMessage: string = '';
    public readonly permissionFlags: PermissionFlagOption[] = [
        { value: 1, display: 'Read', mask: '00001' },
        { value: 2, display: 'Write', mask: '00010' },
        { value: 4, display: 'Create', mask: '00100' },
        { value: 8, display: 'Delete', mask: '01000' },
        { value: 16, display: 'Admin', mask: '10000' }
    ];

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
            name: 'selectedFlags',
            type: 'multiselect',
            label: 'Permission Flags',
            required: true,
            options: this.permissionFlags,
            validators: [Validators.required]
        },
        {
            name: 'description',
            type: 'text',
            label: 'Description',
            required: false
        }
    ];

    private dataSubscription: any;

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
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        
        // Reset to first page when sorting changes
        this.sort.sortChange.subscribe(() => {
            this.paginator.pageIndex = 0;
        });
        
        // Add direct subscription to page size changes
        this.paginator.page.subscribe(event => {
            console.log('Paginator event triggered:', event);
            // Force reload on any pagination event
            this.setupSearchAndSort();
        });
        
        this.loadPermissions();
    }

    public loadPermissions(): void {
        this.setupSearchAndSort();
    }

    private setupSearchAndSort(): void {
        // Unsubscribe from previous subscriptions if any
        if (this.dataSubscription) {
            this.dataSubscription.unsubscribe();
        }
        
        this.dataSubscription = merge(
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
                    
                    console.log('Paginator event:', {
                        pageIndex: this.paginator.pageIndex,
                        pageSize: this.paginator.pageSize,
                        length: this.paginator.length
                    });
                    
                    const params: QueryParams = {
                        page: this.paginator.pageIndex + 1,
                        pageSize: this.paginator.pageSize,
                        searchTerm: this.currentSearchValue,
                        sortColumn: this.sort.active,
                        sortDirection: this.sort.direction as 'asc' | 'desc'
                    };
                    
                    console.log('API request params:', params);

                    return this.currentSearchValue && this.currentSearchValue.length >= 3
                        ? this.permissionService.findPermissionsByTerm(this.currentSearchValue)
                        : this.permissionService.getPermissions(this.queryService.normaliseQueryParams(params));
                }),
                catchError(error => {
                    console.error('Error fetching permissions:', error);
                    NotificationUtils.showError(this.dialog, 'Failed to fetch permissions', error);
                    return of({items: [], total: 0, page: 1, pageSize: 50});
                })
            )
            .subscribe((response: any) => {
                console.log('API response:', {
                    total: response.total,
                    itemCount: response.items.length,
                    page: response.page,
                    pageSize: response.pageSize
                });
                
                this.dataSource.data = response.items;
                this.totalItems = response.total;
                this.isLoading = false;
            });
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
                switchMap(result => {
                    const permissionString = this.convertFlagsToPermissionString(result.selectedFlags);
                    return this.permissionService.create({
                        ...result,
                        permissionString
                    });
                })
            )
            .subscribe({
                next: () => {
                    this.loadPermissions();
                    NotificationUtils.showSuccess(this.dialog, 'Permission has been created successfully');
                },
                error: (error) => {
                    console.error('Error creating permission:', error);
                    NotificationUtils.showError(this.dialog, 'Failed to create permission', error);
                }
            });
    }

    public onEdit(permission: PermissionResponse): void {
        const selectedFlags = this.convertPermissionStringToFlags(permission.permissionString);
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
                    selectedFlags
                }
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter(result => !!result),
                switchMap(result => {
                    const permissionString = this.convertFlagsToPermissionString(result.selectedFlags);
                    return this.permissionService.update({
                        id: permission.id,
                        name: result.name,
                        description: result.description,
                        permissionString
                    });
                })
            )
            .subscribe({
                next: () => {
                    this.loadPermissions();
                    NotificationUtils.showSuccess(this.dialog, `Permission "${permission.name}" has been updated successfully`);
                },
                error: (error) => {
                    console.error('Error updating permission:', error);
                    NotificationUtils.showError(this.dialog, 'Failed to update permission', error);
                }
            });
    }

    private convertFlagsToPermissionString(selectedFlags: number[]): string {
        let result = 0;
        for (const flag of selectedFlags) {
            result |= flag;
        }
        return result.toString(2).padStart(5, '0');
    }

    private convertPermissionStringToFlags(permissionString: string): number[] {
        const flags: number[] = [];
        const value = parseInt(permissionString, 2);
        
        for (const flag of this.permissionFlags) {
            if ((value & flag.value) === flag.value) {
                flags.push(flag.value);
            }
        }
        return flags;
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
                            NotificationUtils.showError(this.dialog, `Failed to delete permission ${permission.name}`, error);
                            return of(null);
                        })
                    )
                    .subscribe(() => {
                        this.loadPermissions();
                        NotificationUtils.showSuccess(this.dialog, `Permission "${permission.name}" has been deleted successfully`);
                    });
            }
        });
    }

    public openEntityDialog(roleIds: number[]): void {
        if (roleIds.length === 0) {
            NotificationUtils.showInfo(this.dialog, 'No roles assigned to this permission');
            return;
        }

        this.roleService.searchRoles({ids: roleIds}).subscribe((roles) => {
            const data = roles.items.map(role => ({
                id: role.id,
                name: role.name
            }));
            this.openEntitySelectorDialog(data, [
                {id: 'name', label: 'Name'}
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
}
