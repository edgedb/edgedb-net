using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents the different error codes sent by the server defined 
    ///     <seealso href="https://www.edgedb.com/docs/reference/protocol/errors#error-codes">in the docs.</seealso>
    /// </summary>
    public enum ServerErrorCodes : uint
    {
        #region Server Errors (0x01)
        InternalServerError = 0x_01_00_00_00,
        #endregion

        #region Feature Errors (0x02)
        UnsupportedFeatureError = 0x_02_00_00_00,
        #endregion

        #region Protocol Errors (0x03)
        ProtocolError = 0x_03_00_00_00,
        
        BinaryProtocolError = 0x_03_01_00_00,
        UnsupportedProtocolVersionError = 0x_03_01_00_01,
        TypeSpecNotFoundError = 0x_03_01_00_02,
        UnexpectedMessageError = 0x_03_01_00_03,

        InputDataError = 0x_03_02_00_00,
        ParameterTypeMismatchError = 0x_03_02_01_00,
        StateMismatchError = 0x_03_02_02_00,

        ResultCardinalityMismatchError = 0x_03_03_00_00,

        CapabilityError = 0x_03_04_00_00,
        UnsupportedCapabilityError = 0x_03_04_01_00,
        DisabledCapabilityError = 0x_03_04_02_00,
        #endregion

        #region Query Errors (0x04 - 0x0405)
        QueryError = 0x_04_00_00_00,

        InvalidSyntaxError = 0x_04_01_00_00,
        EdgeQLSyntaxError = 0x_04_01_01_00,
        SchemaSyntaxError = 0x_04_01_02_00,
        GraphQLSyntaxError = 0x_04_01_03_00,

        InvalidTypeError = 0x_04_02_00_00,
        InvalidTargetError = 0x_04_02_01_00,
        InvalidLinkTargetError = 0x_04_02_01_01,
        InvalidPropertyTargetError = 0x_04_02_01_02,

        InvalidReferenceError = 0x_04_03_00_00,
        UnknownModuleError = 0x_04_03_00_01,
        UnknownLinkError = 0x_04_03_00_02,
        UnknownPropertyError = 0x_04_03_00_03,
        UnknownUserError = 0x_04_03_00_04,
        UnknownDatabaseError = 0x_04_03_00_05,
        UnknownParameterError = 0x_04_03_00_06,

        SchemaError = 0x_04_04_00_00,

        SchemaDefinitionError = 0x_04_05_00_00,

        InvalidDefinitionError = 0x_04_05_01_00,
        InvalidModuleDefinitionError = 0x_04_05_01_01,
        InvalidLinkDefinitionError = 0x_04_05_01_02,
        InvalidPropertyDefinitionError = 0x_04_05_01_03,
        InvalidUserDefinitionError = 0x_04_05_01_04,
        InvalidDatabaseDefinitionError = 0x_04_05_01_05,
        InvalidOperatorDefinitionError = 0x_04_05_01_06,
        InvalidAliasDefinitionError = 0x_04_05_01_07,
        InvalidFunctionDefinitionError = 0x_04_05_01_08,
        InvalidConstraintDefinitionError = 0x_04_05_01_09,
        InvalidCastDefinitionError = 0x_04_05_01_0A,

        DuplicateDefinitionError = 0x_04_05_02_00,
        DuplicateModuleDefinitionError = 0x_04_05_02_01,
        DuplicateLinkDefinitionError = 0x_04_05_02_02,
        DuplicatePropertyDefinitionError = 0x_04_05_02_03,
        DuplicateUserDefinitionError = 0x_04_05_02_04,
        DuplicateDatabaseDefinitionError = 0x_04_05_02_05,
        DuplicateOperatorDefinitionError = 0x_04_05_02_06,
        DuplicateViewDefinitionError = 0x_04_05_02_07,
        DuplicateFunctionDefinitionError = 0x_04_05_02_08,
        DuplicateConstraintDefinitionError = 0x_04_05_02_09,
        DuplicateCastDefinitionError = 0x_04_05_02_0A,
        #endregion

        #region Timeout errors (0x0406)
        SessionTimeoutError = 0x_04_06_00_00,

        [ShouldReconnect]
        IdleSessionTimeoutError = 0x_04_06_01_00,
        
        QueryTimeoutError = 0x_04_06_02_00,

        TransactionTimeoutError = 0x_04_06_0A_00,
        IdleTransactionTimeoutError = 0x_04_06_0A_01,
        #endregion

        #region General Errors (0x05)
        ExecutionError = 0x_05_00_00_00,

        InvalidValueError = 0x_05_01_00_00,
        DivisionByZeroError = 0x_05_01_00_01,
        NumericOutOfRangeError = 0x_05_01_00_02,
        AccessPolicyError = 0x_05_01_00_03,

        IntegrityError = 0x_05_02_00_00,
        ConstraintViolationError = 0x_05_02_00_01,
        CardinalityViolationError = 0x_05_02_00_02,
        MissingRequiredError = 0x_05_02_00_03,

        TransactionError = 0x_05_03_00_00,
        [ShouldRetry]
        TransactionConflictError = 0x_05_03_01_00,
        [ShouldRetry]
        TransactionSerializationError = 0x_05_03_01_01,
        [ShouldRetry]
        TransactionDeadlockError = 0x_05_03_01_02,
        #endregion

        #region Config Errors (0x06)
        ConfigurationError = 0x_06_00_00_00,
        #endregion

        #region Auth Errors (0x07)
        AccessError = 0x_07_00_00_00,

        AuthenticationError = 0x_07_01_00_00,
        #endregion

        #region Availability Errors (0x08)
        AvailabilityError = 0x_08_00_00_00,
        [ShouldRetry]
        BackendUnavailableError = 0x_08_00_00_01,
        #endregion

        #region Backend Errors (0x09)
        BackendError = 0x_09_00_00_00,

        UnsupportedBackendFeatureError = 0x_09_00_01_00,
        #endregion

        #region Messages (0xF0)
        LogMessage = 0x_F0_00_00_00,

        WarningMessage = 0x_F0_01_00_00,
        #endregion
    }
}
