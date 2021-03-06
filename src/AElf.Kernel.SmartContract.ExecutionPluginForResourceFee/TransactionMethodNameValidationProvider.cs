using AElf.Contracts.MultiToken;
using AElf.Kernel.FeeCalculation.Application;
using AElf.Kernel.SmartContract.Application;

namespace AElf.Kernel.SmartContract.ExecutionPluginForResourceFee
{
    internal class TransactionMethodNameValidationProvider : TokenContractTransactionValidationProviderBase
    {
        public override bool ValidateWhileSyncing => true;
        protected override string[] InvolvedSmartContractMethods { get; }

        public TransactionMethodNameValidationProvider(ISmartContractAddressService smartContractAddressService) : base(
            smartContractAddressService)
        {
            InvolvedSmartContractMethods = new[]
            {
                nameof(TokenContractContainer.TokenContractStub.ChargeResourceToken),
                nameof(TokenContractContainer.TokenContractStub.CheckResourceToken)
            };
        }
    }
}