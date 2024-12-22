import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EntityDisplayDialogComponent } from './entity-display-dialog.component';

describe('EntityDisplayDialogComponent', () => {
  let component: EntityDisplayDialogComponent;
  let fixture: ComponentFixture<EntityDisplayDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EntityDisplayDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EntityDisplayDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
