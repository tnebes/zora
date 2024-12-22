import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {MatPaginator} from '@angular/material/paginator';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {merge, of, Subject} from 'rxjs';
import {catchError, debounceTime, distinctUntilChanged, filter, startWith, switchMap} from 'rxjs/operators';
import {QueryParams} from '../../core/models/query-params.interface';
import {CreateUser, UpdateUser, UserResponse} from '../../core/models/user.interface';
import {UserService} from '../../core/services/user.service';
import {MatDialog} from '@angular/material/dialog';
import {ConfirmDialogComponent} from 'src/app/shared/components/confirm-dialog/confirm-dialog.component';
import {BaseDialogComponent, DialogField} from 'src/app/shared/components/base-dialog/base-dialog.component';
import {Validators} from '@angular/forms';
import {RoleService} from 'src/app/core/services/role.service';
import {RoleResponse} from "../../core/models/role.interface";
import {DefaultValues} from "../../core/constants";
import {QueryService} from "../../core/services/query.service";

@Component({
    selector: 'app-users',
    templateUrl: './users.component.html',
    styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit, AfterViewInit {
    public readonly displayedColumns: string[] = ['username', 'email', 'createdAt', 'roles', 'actions'];
    public readonly dataSource: MatTableDataSource<UserResponse> = new MatTableDataSource();
    public totalItems: number = 0;
    public isLoading: boolean = false;
    public searchTerm: Subject<string> = new Subject<string>();
    private currentSearchValue: string = '';
    @ViewChild(MatPaginator) private readonly paginator!: MatPaginator;
    @ViewChild(MatSort) private readonly sort!: MatSort;

    private readonly userFields: DialogField[] = [
        {
            name: 'username',
            type: 'text',
            label: 'Username',
            required: true,
            validators: [Validators.minLength(3)]
        },
        {
            name: 'email',
            type: 'email',
            label: 'Email',
            required: true,
            validators: [Validators.email]
        },
        {
            name: 'roles',
            type: 'multiselect',
            label: 'Roles',
            required: true,
            options: []
        }
    ];
    private readonly createUserFields: DialogField[] = [
        ...this.userFields,
        {
            name: 'password',
            type: 'password',
            label: 'Password',
            required: true,
            validators: [Validators.minLength(6)]
        }
    ];

    constructor(
        private readonly userService: UserService,
        private readonly roleService: RoleService,
        private readonly queryService: QueryService,
        private readonly dialog: MatDialog
    ) {
    }

    public ngAfterViewInit(): void {
        this.addRoles();
        this.setupSearchAndSort();
    }

    public ngOnInit(): void {}

    public onSearch(event: Event): void {
        const target = event.target as HTMLInputElement;
        this.currentSearchValue = target.value;
        this.searchTerm.next(this.currentSearchValue);
    }

    public onDelete(user: UserResponse): void {
        const dialogRef = this.dialog.open(ConfirmDialogComponent, {
            width: '400px',
            data: {
                title: 'Confirm Delete',
                message: `Are you sure you want to delete ${user.username}?`,
                confirmButton: 'Delete',
                cancelButton: 'Cancel'
            }
        });

        dialogRef.afterClosed().subscribe((result: boolean) => {
            if (result) {
                this.userService.deleteUser(user.id)
                    .pipe(
                        catchError(error => {
                            console.error('Error deleting user:', error);
                            return of(null);
                        })
                    )
                    .subscribe(response => {
                        if (response !== null) {
                            this.loadUsers();
                        }
                    });
            }
        });
    }

    public onCreate(): void {
        const dialogRef = this.dialog.open(BaseDialogComponent<CreateUser>, {
            width: '500px',
            data: {
                title: 'Create User',
                fields: this.createUserFields,
                mode: 'create'
            }
        });

        dialogRef.afterClosed()
            .pipe(
                switchMap(result => this.userService.createUser(result))
            )
            .subscribe({
                next: () => {
                    this.loadUsers();
                },
                error: (error) => {
                    console.error('Error creating user:', error);
                }
            });
    }

    public onEdit(user: UserResponse): void {
        console.debug('Editing user:', user);
        let updateUser: UpdateUser = {
            id: user.id,
            username: user.username,
            email: user.email,
            roles: this.userService.formatRolesForSelection(user.roles)
        }
        const dialogRef = this.dialog.open(BaseDialogComponent<UpdateUser>, {
            width: '500px',
            data: {
                title: 'Edit User',
                fields: this.userFields,
                mode: 'edit',
                entity: updateUser
            }
        });

        dialogRef.afterClosed()
            .pipe(
                filter(result => !!result),
                switchMap(result => this.userService.updateUser(updateUser))
            )
            .subscribe({
                next: () => {
                    this.loadUsers();
                },
                error: (error) => {
                    console.error('Error updating user:', error);
                }
            });
    }

    formatRoles(roles: { [key: number]: string }): string {
        if (!roles || Object.keys(roles).length === 0) {
            return 'N/A';
        }
        return Object.values(roles).join(', ');
    }

    private setupSearchAndSort(): void {
        merge(
            this.searchTerm.pipe(
                debounceTime(300),
                distinctUntilChanged()
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
                    return this.userService.getUsers(this.queryService.normaliseQueryParams(params));
                })
            )
            .subscribe(response => {
                this.dataSource.data = response.items;
                this.totalItems = response.total;
                this.isLoading = false;
            });
    }

    private loadUsers(): void {
        this.searchTerm.next(this.currentSearchValue);
    }

    private addRoles(): void {
        const rolesField: DialogField | undefined = this.userFields.find(field => field.name === 'roles');
        if (!rolesField) {
            console.error('Roles field not found');
            return;
        }
        // TODO FIXME what happens if a user wishes to choose a role that is not in the list?
        this.roleService.getRoles(DefaultValues.QUERY_PARAMS)
            .subscribe(response => {
                rolesField.options = this.toOptions(response.items);
            });
    }

    private toOptions(items: RoleResponse[]): { value: number, display: string }[] {
        return items.map(item => ({value: item.id, display: item.name}));
    }
}