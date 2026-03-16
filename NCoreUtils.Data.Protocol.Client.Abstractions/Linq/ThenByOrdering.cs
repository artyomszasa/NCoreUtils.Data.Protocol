namespace NCoreUtils.Data.Protocol.Linq;

public readonly struct ThenByOrdering(Ast.Node expression, bool isDescending)
{
    public Ast.Node Expression { get; } = expression;

    public bool IsDescending { get; } = isDescending;
}