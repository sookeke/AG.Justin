﻿namespace AG.Justin.CustomClaim.Api.Features;

public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    new Task<TResult> HandleAsync(TQuery query);
}
