import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {MatTableDataSource} from '@angular/material/table';
import {MatDialog} from '@angular/material/dialog';
import {RoleResponse} from "../../core/models/role.interface";
import {
    EntitySelectorDialogComponent
} from "../../shared/components/entity-display-dialog/entity-display-dialog.component";
import {RoleService} from "../../core/services/role.service";
import {merge, Subject} from "rxjs";
import {debounceTime, distinctUntilChanged, startWith, switchMap} from "rxjs/operators";
import {QueryParams} from "../../core/models/query-params.interface";
import {MatPaginator} from "@angular/material/paginator";
import {MatSort} from "@angular/material/sort";
import {QueryService} from "../../core/services/query.service";
import {UserResponse, UserResponseDto} from "../../core/models/user.interface";
import {UserService} from "../../core/services/user.service";
import {NotificationDialogComponent} from "../../shared/components/notification-dialog/notification-dialog.component";

@Component({
    selector: 'app-roles',
    templateUrl: './roles.component.html',
    styleUrls: ['./roles.component.scss']
})
export class RolesComponent implements OnInit, AfterViewInit {
    public dataSource: MatTableDataSource<RoleResponse> = new MatTableDataSource();
    public displayedColumns: string[] = ['name', 'createdAt', 'permissions', 'roles', 'actions'];
    public searchTerm: Subject<string> = new Subject<string>();
    public isLoading: boolean = false;
    public totalItems: number = 0;
    private currentSearchValue: string = '';

    @ViewChild(MatPaginator) private readonly paginator!: MatPaginator;
    @ViewChild(MatSort) private readonly sort!: MatSort;

    constructor(private dialog: MatDialog,
                private readonly roleService: RoleService,
                private readonly queryService: QueryService,
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
        console.log('Create Role button clicked');
    }

    public onEdit(role: RoleResponse): void {
        console.log('Edit Role:', role);
    }

    public onDelete(role: RoleResponse): void {
        console.log('Delete Role:', role);
    }

    public openEntityDialog(type: 'roles' | 'permissions', userIds: number[]): void {
        if (type === 'roles') {
            let roleIds: number[] = Array.from(
                new Set(userIds)
            );

            if (roleIds.length === 0) {
                this.dialog.open(NotificationDialogComponent, {
                    width: '600px',
                    data: {
                        message: 'No users assigned to this role'
                    }
                });
            }

            this.userService.searchUsers({roles: roleIds}).subscribe((usersDto: UserResponseDto<UserResponse>) => {
                const data = usersDto.items.map(user => {
                    return {
                        id: user.id,
                        name: user.username
                    };
                });
                this.dialog.open(EntitySelectorDialogComponent, {
                    width: '600px',
                    data: {
                        entities: data,
                        columns: [
                            {id: 'name', label: 'Name'},
                            {id: 'id', label: 'ID'}
                        ]
                    }
                });
            });
        }
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
                    return this.roleService.getRoles(this.queryService.normaliseQueryParams(params));
                })
            )
            .subscribe(response => {
                this.dataSource.data = response.items;
                this.totalItems = response.total;
                this.isLoading = false;
            });
    }

}
