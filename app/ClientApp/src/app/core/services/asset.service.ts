import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {AssetResponse, CreateAsset, UpdateAsset, AssetResponseDto} from "../models/asset.interface";
import {QueryParams} from "../models/query-params.interface";
import {Constants} from '../constants';
import {QueryService} from './query.service';

@Injectable({
    providedIn: 'root'
})
export class AssetService {
    private readonly apiUrl: string = Constants.ASSETS;

    constructor(
        private readonly http: HttpClient,
        private readonly queryService: QueryService
    ) {
    }

    public getAssets(queryParams: QueryParams): Observable<AssetResponseDto> {
        const params: HttpParams = this.queryService.getHttpParams(queryParams);
        return this.http.get<AssetResponseDto>(this.apiUrl, {params});
    }

    public create(asset: CreateAsset | FormData): Observable<AssetResponse> {
        if (asset instanceof FormData) {
            return this.http.post<AssetResponse>(this.apiUrl, asset);
        }

        if (!asset?.name || !asset?.asset) {
            throw new Error('Invalid asset data: name and file are required');
        }

        const formData = new FormData();
        formData.append('name', asset.name);
        if (asset.description) {
            formData.append('description', asset.description);
        }
        if (asset.workAssetId) {
            formData.append('workAssetId', asset.workAssetId.toString());
        }
        formData.append('asset', asset.asset, asset.asset.name);

        return this.http.post<AssetResponse>(this.apiUrl, formData);
    }

    public update(asset: UpdateAsset): Observable<AssetResponse> {
        return this.http.put<AssetResponse>(`${this.apiUrl}/${asset.id}`, asset);
    }

    public delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    public findAssetsByTerm(searchTerm: string): Observable<AssetResponseDto> {
        if (!searchTerm || searchTerm.length < 3) {
            throw new Error('Search term must be at least 3 characters long');
        }

        const params = new HttpParams().set('searchTerm', searchTerm);
        return this.http.get<AssetResponseDto>(`${Constants.ASSETS_FIND}`, {params});
    }

    public download(id: number): Observable<Blob> {
        return this.http.get(`${this.apiUrl}/${id}/download`, {
            responseType: 'blob'
        });
    }

    public getAssetsByTaskId(taskId: number): Observable<AssetResponseDto> {
        const params = new HttpParams().set('workItemId', taskId.toString());
        return this.http.get<AssetResponseDto>(`${Constants.ASSETS_SEARCH}`, { params });
    }
}
