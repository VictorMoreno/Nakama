export class ApiClientBase {

    protected async transformOptions(options_: RequestInit): Promise<RequestInit> {
        return options_;
    }

    protected async transformResult<T>(_url: string, response: Response, processor: (_response: Response) => Promise<T>): Promise<T> {
        return processor(response);
    }
}