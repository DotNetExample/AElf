using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Kernel.Blockchain.Domain;
using AElf.Types;
using Volo.Abp.DependencyInjection;

namespace AElf.Kernel.Blockchain.Application
{
    public interface ITransactionResultQueryService
    {
        Task<TransactionResult> GetTransactionResultAsync(Hash transactionId);
        Task<TransactionResult> GetTransactionResultAsync(Hash transactionId, Hash blockHash);
    }

    public interface ITransactionResultService : ITransactionResultQueryService
    {
        Task AddTransactionResultAsync(TransactionResult transactionResult, BlockHeader blockHeader);
        Task AddTransactionResultsAsync(IList<TransactionResult> transactionResult, BlockHeader blockHeader);

        Task ProcessTransactionResultAfterExecutionAsync(BlockHeader blockHeader, List<Hash> transactionIds);
    }


    public class TransactionResultService : ITransactionResultService, ITransientDependency
    {
        private readonly ITransactionResultManager _transactionResultManager;
        private readonly ITransactionBlockIndexService _transactionBlockIndexService;
        private readonly IBlockchainService _blockchainService;

        public TransactionResultService(ITransactionResultManager transactionResultManager,
            IBlockchainService blockchainService, ITransactionBlockIndexService transactionBlockIndexService)
        {
            _transactionResultManager = transactionResultManager;
            _blockchainService = blockchainService;
            _transactionBlockIndexService = transactionBlockIndexService;
        }

        public async Task AddTransactionResultAsync(TransactionResult transactionResult, BlockHeader blockHeader)
        {
            await _transactionResultManager.AddTransactionResultAsync(transactionResult,
                blockHeader.GetDisambiguatingHash());
        }

        public async Task AddTransactionResultsAsync(IList<TransactionResult> transactionResults,
            BlockHeader blockHeader)
        {
            await _transactionResultManager.AddTransactionResultsAsync(transactionResults,
                blockHeader.GetDisambiguatingHash());
        }

        public async Task<TransactionResult> GetTransactionResultAsync(Hash transactionId)
        {
            var transactionBlockIndex =
                await _transactionBlockIndexService.GetTransactionBlockIndexAsync(transactionId);

            if (transactionBlockIndex != null)
                return await _transactionResultManager.GetTransactionResultAsync(transactionId,
                    transactionBlockIndex.BlockHash);

            return null;
        }

        public async Task<TransactionResult> GetTransactionResultAsync(Hash transactionId, Hash blockHash)
        {
            var txResult = await _transactionResultManager.GetTransactionResultAsync(transactionId, blockHash);
            if (txResult != null)
            {
                return txResult;
            }

            var header = await _blockchainService.GetBlockHeaderByHashAsync(blockHash);
            txResult = await _transactionResultManager.GetTransactionResultAsync(transactionId,
                header.GetPreMiningHash());

            return txResult;
        }

        public async Task ProcessTransactionResultAfterExecutionAsync(BlockHeader blockHeader,
            List<Hash> transactionIds)
        {
            var preMiningHash = blockHeader.GetPreMiningHash();
            var blockIndex = new BlockIndex
            {
                BlockHash = blockHeader.GetHash(),
                BlockHeight = blockHeader.Height
            };

            if (transactionIds.Count == 0)
            {
                // This will only happen during test environment
                return;
            }

            var firstTransaction = transactionIds.First();
            var withBlockHash = await _transactionResultManager.HasTransactionResultAsync(
                firstTransaction, blockHeader.GetHash());
            var withPreMiningHash = await _transactionResultManager.HasTransactionResultAsync(
                firstTransaction, preMiningHash);

            if (!withBlockHash)
            {
                // TransactionResult is not saved with real BlockHash
                // Save results with real (post mining) Hash, so that it can be queried with TransactionBlockIndex
                var result = await _transactionResultManager.GetTransactionResultsAsync(transactionIds, preMiningHash);
                await _transactionResultManager.AddTransactionResultsAsync(result, blockIndex.BlockHash);
            }

            // Add TransactionBlockIndex
            if (withPreMiningHash)
            {
                await _transactionResultManager.RemoveTransactionResultsAsync(transactionIds, preMiningHash);
            }

            await _transactionBlockIndexService.AddBlockIndexAsync(transactionIds, blockIndex);
        }
    }
}