class ApiClientBase {

    protected async transformOptions(options_: RequestInit): Promise<RequestInit> {
        return options_;
    }

    protected async transformResult(url: string, response: Response, processor: (_response: Response) => Promise<any>): Promise<any> {
        return processor(response);
    }
}