namespace NCoreUtils.Data.Protocol.Ast;

public enum BinaryOperation
{
    // Comparison ******************************************************************************************************
    /// <summary>
    /// Represents equality check operation.
    /// </summary>
    Equal                = 0,
    /// <summary>
    /// Represents inequality check operation.
    /// </summary>
    NotEqual             = 1,
    /// <summary>
    /// Represents less than comparison operation.
    /// </summary>
    LessThan             = 2,
    /// <summary>
    /// Represents less than or equal comparison operation.
    /// </summary>
    LessThanOrEqual      = 3,
    /// <summary>
    /// Represents grater than comparison operation.
    /// </summary>
    GreaterThan          = 4,
    /// <summary>
    /// Represents grater than or equals comparison operation.
    /// </summary>
    GreaterThanOrEqual   = 5,
    // Conditional *****************************************************************************************************
    /// <summary>
    /// Represents a short-circuiting conditional OR operation.
    /// </summary>
    OrElse               = 6,
    /// <summary>
    /// Represents a conditional AND operation that evaluates the second operand only if the first operand evaluates to true.
    /// </summary>
    AndAlso              = 7,
    // Arithmetic ******************************************************************************************************
    /// <summary>Represents an addition operation.</summary>
    Add                  = 8,
    /// <summary>Represents a substraction operation.</summary>
    Subtract            = 9,
    /// <summary>Represents a multiplication operation.</summary>
    Multiply             = 10,
    /// <summary>Represents a division operation.</summary>
    Divide               = 11,
    /// <summary>Represents an arithmetic remainder operation.</summary>
    Modulo               = 12
    // TODO: Bitwise
}