namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class GenerationOptions
{
    public bool GenerateBox { get; }
    public bool GenerateBoxValueField { get; }
    public bool GenerateBoxNullable { get; }
    public bool GenerateType { get; }
    public bool GenerateArrayOfType { get; }
    public bool GenerateEnumerableOfType { get; }
    public bool GenerateProperties { get; }
    public bool GenerateCreateAdd { get; }
    public bool GenerateCreateAndAlso { get; }
    public bool GenerateCreateBoxedConstant { get; }
    public bool GenerateCreateDivide { get; }
    public bool GenerateCreateGreaterThan { get; }
    public bool GenerateCreateGreaterThanOrEqual { get; }
    public bool GenerateCreateLessThan { get; }
    public bool GenerateCreateLessThanOrEqual { get; }
    public bool GenerateCreateModulo { get; }
    public bool GenerateCreateMultiply { get; }
    public bool GenerateCreateOrElse { get; }
    public bool GenerateCreateSubtract { get; }
    public bool GenerateCreateEqual { get; }
    public bool GenerateCreateNotEqual { get; }
    public bool GenerateIsEnumerable { get; }
    public bool GenerateIsArray { get; }
    public bool GenerateIsLambda { get; }
    public bool GenerateIsMaybe { get; }
    public bool GenerateIsNullable { get; }
    public bool GenerateTryGetEnumFactory { get; }
    public bool GenerateEnumerableAnyMethod { get; }
    public bool GenerateEnumerableAllMethod { get; }
    public bool GenerateEnumerableContainsMethod { get; }
    public bool GenerateAccept { get; }

    public bool IsEmpty
        => !GenerateBox
            && !GenerateBoxValueField
            && !GenerateBoxNullable
            && !GenerateType
            && !GenerateArrayOfType
            && !GenerateEnumerableOfType
            && !GenerateProperties
            && !GenerateCreateAdd
            && !GenerateCreateAndAlso
            && !GenerateCreateBoxedConstant
            && !GenerateCreateDivide
            && !GenerateCreateGreaterThan
            && !GenerateCreateGreaterThanOrEqual
            && !GenerateCreateLessThan
            && !GenerateCreateLessThanOrEqual
            && !GenerateCreateModulo
            && !GenerateCreateMultiply
            && !GenerateCreateOrElse
            && !GenerateCreateSubtract
            && !GenerateCreateEqual
            && !GenerateCreateNotEqual
            && !GenerateIsEnumerable
            && !GenerateIsArray
            && !GenerateIsLambda
            && !GenerateIsMaybe
            && !GenerateIsNullable
            && !GenerateTryGetEnumFactory
            && !GenerateEnumerableAnyMethod
            && !GenerateEnumerableAllMethod
            && !GenerateEnumerableContainsMethod
            && !GenerateAccept;

    public GenerationOptions(
        bool generateBox,
        bool generateBoxValueField,
        bool generateBoxNullable,
        bool generateType,
        bool generateArrayOfType,
        bool generateEnumerableOfType,
        bool generateProperties,
        bool generateCreateAdd,
        bool generateCreateAndAlso,
        bool generateCreateBoxedConstant,
        bool generateCreateDivide,
        bool generateCreateGreaterThan,
        bool generateCreateGreaterThanOrEqual,
        bool generateCreateLessThan,
        bool generateCreateLessThanOrEqual,
        bool generateCreateModulo,
        bool generateCreateMultiply,
        bool generateCreateOrElse,
        bool generateCreateSubtract,
        bool generateCreateEqual,
        bool generateCreateNotEqual,
        bool generateIsEnumerable,
        bool generateIsArray,
        bool generateIsLambda,
        bool generateIsMaybe,
        bool generateIsNullable,
        bool generateTryGetEnumFactory,
        bool generateEnumerableAnyMethod,
        bool generateEnumerableAllMethod,
        bool generateEnumerableContainsMethod,
        bool generateAccept)
    {
        GenerateBox = generateBox;
        GenerateBoxNullable = generateBoxNullable;
        GenerateBoxValueField = generateBoxValueField;
        GenerateType = generateType;
        GenerateArrayOfType = generateArrayOfType;
        GenerateEnumerableOfType = generateEnumerableOfType;
        GenerateProperties = generateProperties;
        GenerateCreateAdd = generateCreateAdd;
        GenerateCreateAndAlso = generateCreateAndAlso;
        GenerateCreateBoxedConstant = generateCreateBoxedConstant;
        GenerateCreateDivide = generateCreateDivide;
        GenerateCreateGreaterThan = generateCreateGreaterThan;
        GenerateCreateGreaterThanOrEqual = generateCreateGreaterThanOrEqual;
        GenerateCreateLessThan = generateCreateLessThan;
        GenerateCreateLessThanOrEqual = generateCreateLessThanOrEqual;
        GenerateCreateModulo = generateCreateModulo;
        GenerateCreateMultiply = generateCreateMultiply;
        GenerateCreateOrElse = generateCreateOrElse;
        GenerateCreateSubtract = generateCreateSubtract;
        GenerateCreateEqual = generateCreateEqual;
        GenerateCreateNotEqual = generateCreateNotEqual;
        GenerateIsEnumerable = generateIsEnumerable;
        GenerateIsArray = generateIsArray;
        GenerateIsLambda = generateIsLambda;
        GenerateIsMaybe = generateIsMaybe;
        GenerateIsNullable = generateIsNullable;
        GenerateTryGetEnumFactory = generateTryGetEnumFactory;
        GenerateEnumerableAnyMethod = generateEnumerableAnyMethod;
        GenerateEnumerableAllMethod = generateEnumerableAllMethod;
        GenerateEnumerableContainsMethod = generateEnumerableContainsMethod;
        GenerateAccept = generateAccept;
    }
}