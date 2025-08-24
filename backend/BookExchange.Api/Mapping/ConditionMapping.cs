using System;
using BookExchange.Api.Dtos;
using BookExchange.Api.Entities;

namespace BookExchange.Api.Mapping;

public static class ConditionMapping
{
    public static ConditionDto ToDto(this Condition condition)
    {
        return new ConditionDto(condition.Id, condition.Name);
    }
}
