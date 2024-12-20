import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {MatPaginator} from '@angular/material/paginator';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {merge, of, Subject} from 'rxjs';
import {debounceTime, distinctUntilChanged, switchMap, startWith, catchError, filter} from 'rxjs/operators';
import {UserQueryParams} from '../../core/models/user-query-params.interface';
import {User} from '../../core/models/user.interface';
import {UserService} from '../../core/services/user.service';
import {MatDialog} from '@angular/material/dialog';
import {ConfirmDialogComponent} from 'src/app/shared/components/confirm-dialog/confirm-dialog.component';
import {BaseDialogComponent, DialogField} from 'src/app/shared/components/base-dialog/base-dialog.component';
import {Validators} from '@angular/forms';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit, AfterViewInit {
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
      options: [
        {value: 'admin', display: 'Administrator'},
        {value: 'user', display: 'User'},
        {value: 'guest', display: 'Guest'}
      ]
    }
  ];
  public readonly displayedColumns: string[] = ['username', 'email', 'createdAt', 'roles', 'actions'];
  public readonly dataSource: MatTableDataSource<User> = new MatTableDataSource();
  public totalItems: number = 0;
  public isLoading: boolean = false;
  public searchTerm: Subject<string> = new Subject<string>();

  private currentSearchValue: string = '';

  @ViewChild(MatPaginator) private paginator!: MatPaginator;
  @ViewChild(MatSort) private sort!: MatSort;

  constructor(
    private readonly userService: UserService,
    private readonly dialog: MatDialog
  ) {
  }

  public ngAfterViewInit(): void {
    this.setupSearchAndSort();
  }

  public ngOnInit(): void {
  }

  public onSearch(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.currentSearchValue = target.value;
    this.searchTerm.next(this.currentSearchValue);
  }

  public onDelete(user: User): void {
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
    const dialogRef = this.dialog.open(BaseDialogComponent<User>, {
      width: '500px',
      data: {
        title: 'Create User',
        fields: this.userFields,
        mode: 'create'
      }
    });

    dialogRef.afterClosed()
      .pipe(
        filter(result => !!result),
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

  public onEdit(user: User): void {
    const dialogRef = this.dialog.open(BaseDialogComponent<User>, {
      width: '500px',
      data: {
        title: 'Edit User',
        fields: this.userFields,
        mode: 'edit',
        entity: user
      }
    });

    dialogRef.afterClosed()
      .pipe(
        filter(result => !!result),
        switchMap(result => this.userService.updateUser(user.id, result))
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
          const params: UserQueryParams = {
            page: this.paginator.pageIndex + 1,
            pageSize: this.paginator.pageSize,
            searchTerm: this.currentSearchValue,
            sortColumn: this.sort.active,
            sortDirection: this.sort.direction as 'asc' | 'desc'
          };
          return this.userService.getUsers(params);
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
}
