using EasyExtensions.Extensions;

namespace EasyExtensions.Tests
{
    public class DateTimeTests
    {
        [Test]
        public void ToUnixTimestampMilliseconds_ValidInput_ValidOutput()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 52, 185, DateTimeKind.Utc);
            Assert.That(dateTime.ToUnixTimestampMilliseconds(), Is.EqualTo(1609459252185));
        }

        [Test]
        public void ToUnixTimestampSeconds_ValidInput_ValidOutput()
        {
            var dateTime = new DateTime(2024, 8, 7, 20, 34, 55, DateTimeKind.Utc);
            Assert.That(dateTime.ToUnixTimestampSeconds(), Is.EqualTo(1723062895));
        }

        [Test]
        public void DropMilliseconds_ValidInput_ValidOutput()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, 123, DateTimeKind.Utc);
            Assert.That(dateTime.DropMilliseconds(), Is.EqualTo(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void DropMillisecondsOffset_ValidInput_ValidOutput()
        {
            var dateTimeOffset = new DateTimeOffset(2021, 1, 1, 0, 0, 0, 123, TimeSpan.Zero);
            Assert.That(dateTimeOffset.DropMilliseconds(), Is.EqualTo(new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        }

        [Test]
        public void ToUniversalTimeWithoutOffset_ValidInput_ValidOutput()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Assert.That(dateTime.ToUniversalTimeWithoutOffset(), Is.EqualTo(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void ToUniversalTimeWithoutOffset_ValidInput_ValidKind()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Assert.That(dateTime.ToUniversalTimeWithoutOffset().Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void ToUniversalTimeWithoutOffset_ValidInput_ValidTicks()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Assert.That(dateTime.ToUniversalTimeWithoutOffset().Ticks, Is.EqualTo(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks));
        }

        [Test]
        public void ToUniversalTimeWithoutOffset_ValidInput_ValidKindOffset()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Assert.That(dateTime.ToUniversalTimeWithoutOffset().Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void ToUniversalTimeWithoutOffset_ValidInput_ValidTicksOffset()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Assert.That(dateTime.ToUniversalTimeWithoutOffset().Ticks, Is.EqualTo(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks));
        }

        [Test]
        public void ToUniversalTimeWithoutOffset_ValidInput_ValidKindUtc()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.That(dateTime.ToUniversalTimeWithoutOffset().Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void ToUniversalTimeWithoutOffset_ValidInput_ValidTicksUtc()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.That(dateTime.ToUniversalTimeWithoutOffset().Ticks, Is.EqualTo(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks));
        }

        [Test]
        public void ToUniversalTimeWithoutOffset_ValidInput_ValidKindUnspecified()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            Assert.That(dateTime.ToUniversalTimeWithoutOffset().Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void ToUniversalTimeWithoutOffset_ValidInput_ValidTicksUnspecified()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            Assert.That(dateTime.ToUniversalTimeWithoutOffset().Ticks, Is.EqualTo(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks));
        }

        [Test]
        public void ToUniversalTimeWithoutOffset_ValidInput_ValidKindLocal()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Assert.That(dateTime.ToUniversalTimeWithoutOffset().Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void ToUniversalTimeWithoutOffset_ValidInput_ValidTicksLocal()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Assert.That(dateTime.ToUniversalTimeWithoutOffset().Ticks, Is.EqualTo(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks));
        }

        [Test]
        public void ToUnixTimestampMilliseconds_InvalidInput_ThrowArgumentException()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Assert.Throws(typeof(ArgumentException), () => dateTime.ToUnixTimestampMilliseconds());
        }

        [Test]
        public void ToUnixTimestampSeconds_InvalidInput_ThrowArgumentException()
        {
            var dateTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Assert.Throws(typeof(ArgumentException), () => dateTime.ToUnixTimestampSeconds());
        }
    }
}
