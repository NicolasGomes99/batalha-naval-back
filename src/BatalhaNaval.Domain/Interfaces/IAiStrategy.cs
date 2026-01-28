using BatalhaNaval.Domain.Entities;
using BatalhaNaval.Domain.ValueObjects;

namespace BatalhaNaval.Domain.Interfaces;

public interface IAiStrategy
{
    Coordinate ChooseTarget(Board enemyBoard);
}