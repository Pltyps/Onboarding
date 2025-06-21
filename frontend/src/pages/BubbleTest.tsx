const BubbleTest: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-100 dark:bg-gray-900 flex items-center justify-center p-10">
      <div className="space-y-6 max-w-xl w-full">
        <div className="text-center text-xl font-bold">
          Tailwind Bubble Test
        </div>

        <div className="flex justify-end">
          <div className="bg-byuNavy text-white px-5 py-3 rounded-2xl shadow max-w-[75%]">
            This is a user message bubble
          </div>
        </div>

        <div className="flex justify-start">
          <div className="bg-gray-100 dark:bg-gray-700 text-black dark:text-white px-5 py-3 rounded-2xl shadow max-w-[75%]">
            This is a bot message bubble
          </div>
        </div>
      </div>
    </div>
  );
};

export default BubbleTest;
