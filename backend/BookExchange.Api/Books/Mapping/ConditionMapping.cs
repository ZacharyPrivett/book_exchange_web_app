using BookExchange.Api.Books.Dtos;
using BookExchange.Api.Books.Entities;

namespace BookExchange.Api.Books.Mapping;

public static class ConditionMapping
{
    public static ConditionDto ToDto(this Condition condition)
    {
        return new ConditionDto(condition.Id, condition.Name);
    }
}
