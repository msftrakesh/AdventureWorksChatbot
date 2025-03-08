

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class FakeEmbeddingGenerator : ITextEmbeddingGenerationService
{
    // ✅ Return random embeddings for testing
    public Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        var embeddings = data
            .Select(_ => new ReadOnlyMemory<float>(Enumerable.Range(0, 768)
                .Select(_ => (float)new Random().NextDouble()).ToArray()))
            .ToList();

        return Task.FromResult<IList<ReadOnlyMemory<float>>>(embeddings);
    }

    public int VectorSize => 768;

    public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();
}
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.