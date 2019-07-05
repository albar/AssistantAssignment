using System;
using System.Collections.Generic;

namespace Albar.AssistantAssignment.Abstractions
{
    public interface IGenotypePhenotypeMapper<T> where T : Enum
    {
        IDataRepository DataRepository { get; }
        
        IEnumerable<IScheduleSolutionRepresentation> ToSolution(IAssignmentChromosome<T> chromosome);
        IEnumerable<IScheduleSolutionRepresentation> ToSolution(byte[] genotype);
        IAssignmentChromosome<T> ToChromosome(IEnumerable<IScheduleSolutionRepresentation> solution);
        IAssignmentChromosome<T> ToChromosome(byte[] genotype);
        byte[] NormalizeGene(byte[] bytes);
    }
}