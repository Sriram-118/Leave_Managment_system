// src/app/services/leave.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateLeaveRequest, LeaveRequestResponse,
  LeaveBalance, ReviewRequest, LeaveType
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class LeaveService {
  private apiUrl = 'http://localhost:5000/api/leave';

  constructor(private http: HttpClient) {}

  applyLeave(data: CreateLeaveRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/apply`, data);
  }

  getMyRequests(): Observable<LeaveRequestResponse[]> {
    return this.http.get<LeaveRequestResponse[]>(`${this.apiUrl}/my-requests`);
  }

  getMyBalance(year?: number): Observable<LeaveBalance[]> {
    let params = new HttpParams();
    if (year) params = params.set('year', year.toString());
    return this.http.get<LeaveBalance[]>(`${this.apiUrl}/my-balance`, { params });
  }

  cancelLeave(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/cancel/${id}`);
  }

  getPendingReviews(): Observable<LeaveRequestResponse[]> {
    return this.http.get<LeaveRequestResponse[]>(`${this.apiUrl}/pending-reviews`);
  }

  reviewLeave(id: number, data: ReviewRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/review/${id}`, data);
  }

  getAllRequests(): Observable<LeaveRequestResponse[]> {
    return this.http.get<LeaveRequestResponse[]>(`${this.apiUrl}/all`);
  }

  calculateDays(start: string, end: string): number {
    if (!start || !end) return 0;
    const diff = new Date(end).getTime() - new Date(start).getTime();
    return Math.floor(diff / (1000 * 60 * 60 * 24)) + 1;
  }
}


// src/app/services/notification.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Notification } from '../models/models';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private apiUrl = 'http://localhost:5000/api/notification';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Notification[]> {
    return this.http.get<Notification[]>(this.apiUrl);
  }

  markRead(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/mark-read/${id}`, {});
  }

  markAllRead(): Observable<any> {
    return this.http.put(`${this.apiUrl}/mark-all-read`, {});
  }
}
