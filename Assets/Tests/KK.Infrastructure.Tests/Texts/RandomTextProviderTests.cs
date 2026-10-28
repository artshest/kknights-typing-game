using System;
using System.Collections.Generic;
using KK.Infrastructure.Texts;
using NUnit.Framework;

namespace KK.Infrastructure.Tests.Texts
{
    public class RandomTextProviderTests
    {
        private static readonly string[] Texts = { "один", "два", "три" };

        [Test]
        public void GetTextAsync_ReturnsTextFromList()
        {
            var provider = new RandomTextProvider(Texts, new Random(0));

            for (int i = 0; i < 20; i++)
            {
                string text = provider.GetTextAsync().GetAwaiter().GetResult();
                Assert.That(Texts, Does.Contain(text));
            }
        }

        [Test]
        public void GetTextAsync_CalledRepeatedly_NeverRepeatsPreviousText()
        {
            var provider = new RandomTextProvider(Texts, new Random(0));
            string previous = provider.GetTextAsync().GetAwaiter().GetResult();

            for (int i = 0; i < 100; i++)
            {
                string current = provider.GetTextAsync().GetAwaiter().GetResult();
                Assert.That(current, Is.Not.EqualTo(previous), $"Повтор на вызове {i + 2}");
                previous = current;
            }
        }

        [Test]
        public void GetTextAsync_CalledRepeatedly_ReturnsEveryText()
        {
            var provider = new RandomTextProvider(Texts, new Random(0));
            var seen = new HashSet<string>();

            for (int i = 0; i < 100; i++)
                seen.Add(provider.GetTextAsync().GetAwaiter().GetResult());

            Assert.That(seen, Is.EquivalentTo(Texts));
        }

        [Test]
        public void GetTextAsync_SingleText_ReturnsItEveryTime()
        {
            var provider = new RandomTextProvider(new[] { "единственный" }, new Random(0));

            for (int i = 0; i < 5; i++)
                Assert.That(provider.GetTextAsync().GetAwaiter().GetResult(), Is.EqualTo("единственный"));
        }

        [Test]
        public void Constructor_EmptyList_Throws()
        {
            Assert.Throws<ArgumentException>(() => new RandomTextProvider(Array.Empty<string>(), new Random(0)));
        }

        [Test]
        public void Constructor_NullList_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new RandomTextProvider(null, new Random(0)));
        }
    }
}
