using SkipList.Generic;

namespace SkipListTests
{
    public class SkipListNodeTests
    {
        [Test]
        public void Constructor_ShouldThrowException_WithLessThanOneLayer()
        {
            try
            {
                SkipListNode<int> skipListNode1 = new SkipListNode<int>(1, 0);
                Assert.Fail($"{nameof(skipListNode1)} should've thrown an exception.");
            } catch (ArgumentOutOfRangeException ex)
            {
            } catch (Exception ex)
            {
                Assert.Fail($"Generic Exception thrown instead of ArgumentOutOfRangeException.");
            }

            try
            {
                SkipListNode<int> skipListNode2 = new SkipListNode<int>(1, -5);
                Assert.Fail($"{nameof(skipListNode2)} should've thrown an exception.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
            } catch (Exception ex)
            {
                Assert.Fail($"Generic Exception thrown instead of ArgumentOutOfRangeException.");
            }
            
        }
    }
}
