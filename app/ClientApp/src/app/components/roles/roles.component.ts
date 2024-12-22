import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import {RoleResponse, RoleResponseDto} from "../../core/models/role.interface";
import {
  EntitySelectorDialogComponent
} from "../../shared/components/entity-display-dialog/entity-display-dialog.component";

@Component({
  selector: 'app-roles',
  templateUrl: './roles.component.html',
  styleUrls: ['./roles.component.scss']
})
export class RolesComponent implements OnInit {
  dataSource: MatTableDataSource<RoleResponse> = new MatTableDataSource();
  displayedColumns: string[] = ['name', 'createdAt', 'permissions', 'roles', 'actions'];
  totalItems: number = 0;

  constructor(private dialog: MatDialog) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    const mockData: RoleResponseDto = {
      items: [
        {
          id: 1,
          name: 'Admin',
          createdAt: new Date('2023-01-01'),
          userIds: [101, 102],
          permissionIds: [201, 202]
        },
        {
          id: 2,
          name: 'Editor',
          createdAt: new Date('2023-02-01'),
          userIds: [103, 104],
          permissionIds: [203, 204]
        },
        {
          id: 3,
          name: 'Viewer',
          createdAt: new Date('2023-03-01'),
          userIds: [105, 106],
          permissionIds: [205, 206]
        }
      ],
      total: 3,
      page: 1,
      pageSize: 50
    };
    this.dataSource.data = mockData.items;
    this.totalItems = mockData.total;
  }

  onSearch(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  onCreate(): void {
    console.log('Create Role button clicked');
  }

  onEdit(role: RoleResponse): void {
    console.log('Edit Role:', role);
  }

  onDelete(role: RoleResponse): void {
    console.log('Delete Role:', role);
  }

  openEntityDialog(type: 'roles' | 'permissions'): void {
    const data = this.dataSource.data.map(role => ({
      id: role.id,
      name: role.name,
      type
    }));

    this.dialog.open(EntitySelectorDialogComponent, {
      data: {
        entities: data,
        columns: [
          { id: 'name', label: 'Name' },
          { id: 'id', label: 'ID' }
        ]
      }
    });
  }
}
