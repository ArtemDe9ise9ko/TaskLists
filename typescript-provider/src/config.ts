export interface ApiProviderConfig {
  baseUrl: string;
  getUserId: () => string | Promise<string>;
  defaultHeaders?: Record<string, string>;
  fetchFn?: typeof fetch;
}
