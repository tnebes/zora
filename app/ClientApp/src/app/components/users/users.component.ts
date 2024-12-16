import {Component, OnInit, ViewChild} from '@angular/core';
import {MatPaginator} from '@angular/material/paginator';
import {MatSort} from '@angular/material/sort';
import {MatTableDataSource} from '@angular/material/table';
import {merge, Subject} from 'rxjs';
import {debounceTime, distinctUntilChanged, switchMap, startWith} from 'rxjs/operators';
import {User, UserQueryParams} from '../../core/models/user.interface';
import {UserService} from '../../core/services/user.service';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit {
  public readonly displayedColumns: string[] = ['username', 'email', 'createdAt', 'roles', 'actions'];
  public readonly dataSource: MatTableDataSource<User> = new MatTableDataSource();
  public totalItems: number = 0;
  public isLoading: boolean = false;
  public searchTerm: Subject<string> = new Subject<string>();

  private currentSearchValue: string = '';

  @ViewChild(MatPaginator) private paginator!: MatPaginator;
  @ViewChild(MatSort) private sort!: MatSort;

  constructor(private readonly userService: UserService) {
  }

  public ngOnInit(): void {
    this.setupSearchAndSort();
  }

  public onSearch(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.currentSearchValue = target.value;
    this.searchTerm.next(this.currentSearchValue);
  }

  public onDelete(user: User): void {
    if (confirm(`Are you sure you want to delete ${user.username}?`)) {
      this.userService.deleteUser(user.id).subscribe(() => {
        this.loadUsers();
      });
    }
  }

  public onEdit(user: User): void {
    throw new Error('Not implemented');
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
